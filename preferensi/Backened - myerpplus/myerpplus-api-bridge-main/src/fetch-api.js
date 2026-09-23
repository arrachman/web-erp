const axios = require("axios");
const { signToken } = require("./utils/jwt");
const { mapRequest, mapOtherParamsRequest } = require("./utils/map-request");
const splitArrayToString = require("./utils/map-array-to-string");
const { sptField, sptLogin, sptParam, sptRow, sptSubParam } = require("./constants/splitter")
const logger = require('./utils/logger');


module.exports = async ({ req, config, accessKey, userId }) => {
  try {
    let strRequestBody = "";
    let strOtherBody = "";

    if (accessKey === "") accessKey = "default";
    const isUpdate = !config["isUpdate"] ? 0 : Number(config["isUpdate"]);
    const id = isUpdate ? req.params.id : 0;
    const transactionId =
      config["paramIdType"] &&
      config["paramIdType"] === "dataId" &&
      req.params.id
        ? req.params.id
        : userId;

    if (config.requestBody) {
      const { body } = req;

      //handle main data
      let mainData = splitArrayToString(
        mapRequest({
          configBody: config.requestBody,
          body,
          userId,
          id,
        })
      );

      if (mainData) {
        strRequestBody += mainData
      }

      //handle detail, serial, batch, and so on
      if (config.requestBody["otherParams"]) {
        strOtherBody = mapOtherParamsRequest({
          configBody: config.requestBody["otherParams"],
          body,
          userId,
          id,
        });

        if (strOtherBody.length > 0) {
          if (strRequestBody != "") {
            strRequestBody += sptSubParam
          }
          strRequestBody += strOtherBody;
        }
      }
    }

    // Pagination StandardValue
    const paginationData = {
      page: 0,
      limit: 0,
      filter: "",
      sort: "",
      formatTgl: "yyyy-MM-dd",
      formatTglWaktu: "yyyy-MM-dd hh:mm:ss",
    };

    if (config.requestQueryParams) {
      const { query } = req;
      config.requestQueryParams.forEach((element) => {
        if(query[element]) {
          paginationData[element] = query[element];
        }
      });
    }

    if (config.defaultFilter) {
      paginationData.filter = Object.entries(config.defaultFilter)
      .map(([key, value]) => {
        if (value === 'dataId' ) {
          value = req.params.id
        }
        return `${key} = ${value}`
      })
      .join('AND ');
    }

    let pagination =
      paginationData.page +
      sptSubParam +
      paginationData.limit +
      sptSubParam +
      paginationData.filter +
      sptSubParam +
      paginationData.sort +
      sptSubParam +
      paginationData.formatTgl +
      sptSubParam +
      paginationData.formatTglWaktu;
    let myERPAPIRequestParam =
      accessKey +
      sptParam +
      config.package +
      sptParam +
      pagination +
      sptParam +
      transactionId +
      sptParam +
      isUpdate +
      sptParam +
      strRequestBody;

    logger.info(JSON.stringify({
      param: myERPAPIRequestParam
    }))
    
    let fetchConfig = {
      method: "post",
      maxBodyLength: Infinity,
      url: process.env.MYERPPLUS_WS_URL,
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
      data: `param=${myERPAPIRequestParam}`,
    };

    const result = await axios.request(fetchConfig);
    const mappedResult = mapResult(result.data, config);
    let newData;

    logger.info(JSON.stringify({
      mappedResult
    }))

    if (mappedResult.isSuccess) {
      let resData = mappedResult.Data.split(sptSubParam);

      let otherDataConfig = {};
      let sortedOtherDataConfig = [];
      if (config["responseBodyParams"]) {
        if (resData.length > 1) {
          otherDataConfig = config["responseBodyParams"]["params"]["otherData"];
          sortedOtherDataConfig = Object.keys(otherDataConfig).sort((a, b) => {
            otherDataConfig[a].order - otherDataConfig[b].order;
          });
        }
  
        for (const [i, element] of resData.entries()) {
          if (i === 0) {
            newData = mapResponseNew({
              config: config["responseBodyParams"],
              result: element,
              errMessage: mappedResult.errMessage,
              transactionId: mappedResult.transactionIdResult
            });
          } else {
            const field = sortedOtherDataConfig[i - 1];
            newData[field] = mapResponseNew({
              config: otherDataConfig[field],
              result: element,
            });
          }
        }
      }
      
    }

    return {
      isSuccess: mappedResult.isSuccess,
      message: mappedResult.errMessage,
      countRow: mappedResult.countRow,
      isNext: mappedResult.isNext,
      isPrev: mappedResult.isPrev,
      currentPage: mappedResult.currentPage,
      data: newData,
    };
  } catch (err) {
    logger.error(err);
    return {
      isSuccess: false,
      message: err.toString(),
    };
  }
};

const mapResult = (data, config) => {
  // split ob_resultWs dg spt Param
  const vArrResult = data.split(sptParam);

  // Split ArrResult dg sptSubParam untuk mendapakan Result, Paging dan data
  const vResult = String(vArrResult[0]).split(sptSubParam);
  const vPaging = String(vArrResult[1]).split(sptSubParam);
  let Data = String(vArrResult[2]);

  // // Set Target, Succes, Errmessage, Errstep dan idtransaksi
  const isSuccess = Boolean(Number(vResult[1]));
  const errMessage = vResult[2];
  const wsErrstep = vResult[3];
  const transactionIdResult = vResult[4];

  // set Ispaging, IsNext, IsPrev, CurPage, CountRow
  const isPaging = Boolean(Number(vPaging[0]));
  const isNext = Boolean(Number(vPaging[1]));
  const isPrev = Boolean(Number(vPaging[2]));
  const currentPage = Number(vPaging[3]);
  const countRow = Number(vPaging[4]);

  if (config.isLoginEndpoint) {
    // split ws login
    const loginData = vArrResult[2].split(sptLogin);
    //loginData[0] is user data, and will concatenated with websiteAccess key in loginData[10]
    Data = loginData[10] + sptField + loginData[0];
  }

  return {
    Data,
    isNext,
    isPrev,
    currentPage,
    countRow,
    isSuccess,
    errMessage,
    transactionIdResult
  };
};

const mapResponseNew = ({ config, result, errMessage, transactionId }) => {
  let newData = [];
  if (config["type"]) {
    let sortedBodyConfig = Object.keys(config.params)
      .sort((a, b) => Number(config.params[a].order) - Number(config.params[b].order))
      .filter((element) => element !== "otherData");

    let transactionNumber = errMessage;
    let dataRows = result.split(sptRow);
    
    const newElement = dataRows[0].split(sptField);
    const objectElement = {};
    switch (config.type) {
      case "array":
        for (const element of dataRows) {
          const newElement = element.split(sptField);

          const objectElement = {};
          for (const [i, el] of sortedBodyConfig.entries()) {
            if(i > newElement.length -1) continue
            objectElement[el] = newElement[i];
          }

          newData.push(objectElement);
        }
        break;
      default:
        for (const [i, el] of sortedBodyConfig.entries()) {
          
          if (config.params[el].type === "transactionNumber") {
            objectElement[el] = transactionNumber;
            continue;
          }
          if (config.params[el].type === "transactionId") {
            objectElement[el] = transactionId;
            continue;
          }

          if(i >= newElement.length -1) continue
          objectElement[el] = newElement[i];
        }
        newData = objectElement;
        newData = config.type === "token" ? signToken(objectElement) : newData;
        break;
    }
  }

  return newData;
};
