const { dateFormatDB } = require("./date-format");
const splitArrayToString = require("./map-array-to-string")
const { sptRow, sptSubParam } = require("./../constants/splitter");
const logger = require("./logger");

const mapRequest = ({ configBody, body, userId, id }) => {
  let sortedBodyConfig = Object.keys(configBody).sort(
    (a, b) => configBody[a].order - configBody[b].order
  );

  const mappedData = [];
  for (const key of sortedBodyConfig) {
    if (key === "otherParams") continue;

    let value = "";
    if (body[key] || body[key] === 0) {
      value = body[key];
    } else if(!body[key]){
      value = configBody[key].default
    }

    switch (configBody[key].default) {
      case "userId":
        value = userId;
        break;
      case "date-time-now":
        value = dateFormatDB(new Date());
        break;
      case "app-key":
        value = process.env.APP_KEY;
        break;
      case "secret-key":
        value = process.env.SECRET_KEY;
        break;
      case "app-code":
        value = process.env.APP_CODE;
        break;
      case "login-replace":
        value = process.env.LOGIN_REPLACE;
        break;
      case "id":
        value = id;
        break;
      
    }

    mappedData.push(value);
  }
  return mappedData;
};

const mapOtherParamsRequest = ({ configBody, body, userId, id }) => {
  let sortedBodyConfig = Object.keys(configBody).sort(
    (a, b) => configBody[a].order - configBody[b].order
  );

  let otherParams = "";
  for (const key of sortedBodyConfig) {
    let strRequestBodyDetail = "";
    if (body[key] || body[key] === 0){
        switch (configBody[key].type) {
            case "array":
              for (const element of body[key]) {
                const detailData = mapRequest({
                  configBody: configBody[key].params,
                  body: element,
                  userId,
                  id,
                });
      
                if (strRequestBodyDetail.length <= 0) {
                  strRequestBodyDetail = splitArrayToString(detailData);
                } else
                  strRequestBodyDetail += sptRow + splitArrayToString(detailData);
              }
              break;
            case "object":
              const detailData = mapRequest({
                configBody: configBody[key].params,
                body: body[key],
                userId,
                id,
              });
      
              strRequestBodyDetail = splitArrayToString(detailData);
              break;
            default:
              strRequestBodyDetail = body[key]
              break
          }
    }

    if (otherParams !== "") {
      otherParams += sptSubParam
    }
    
    otherParams += strRequestBodyDetail;
  }

  return otherParams;
};

module.exports = {
  mapRequest,
  mapOtherParamsRequest,
};
