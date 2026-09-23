const fetchAPI = require("./../fetch-api")

module.exports = async ({ req, res, config }) => {
    const { accessKey, userId } = req.user
    const response = await fetchAPI({ req, config, accessKey, userId })
    if (!response.isSuccess ) {
        const responseCode = response.message === "Invalid Website Access Key." ? 401 : 200
        const responseMessage = response.message === "Invalid Website Access Key." ? 'Unauthorized' : response.message
        
        return res.status(responseCode).json({
            error: responseMessage,
            isSuccess: false
        })
    } 
    
    let metadata
    if (config["needPaginationInfo"]) {
        metadata = {
            countRow: response.countRow,
            isNext: response.isNext,
            isPrev: response.isPrev,
            currentPage: response.currentPage,
        }
    }

    let data
    if(response.data) {
        data = response.data
    }

    res.json({
        isSuccess: true,
        metadata,
        data
    });
}
