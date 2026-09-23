const express = require('express');
const router = express.Router();
const routesConfig = require('./../../api-config/non-authrorized-config.json');
const apiCall = require('./call-api')
routesConfig.forEach((config) => {
    router.get('/', async (req, res) => {
        res.json("Welcome to Public MyERPPlus API");
    }); 

    router[config.method.toLowerCase()](config.route, async (req, res) => {
        req.user = {
            userId: 0,
            accessKey: "",
        }
        return apiCall({ req, res, config })
    });
    
});

module.exports = router;
