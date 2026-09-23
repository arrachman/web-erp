const express = require('express');
const router = express.Router();
const routesConfig = require('../../api-config/purchasing-config.json');
const apiCall = require('./call-api')
routesConfig.forEach((config) => {
    router.get('/', async (req, res) => {
        res.json("Welcome to Purchasing MyERPPlus API");
    }); 
    router[config.method.toLowerCase()](config.route, async (req, res) => {
        return apiCall({ req, res, config })
    });
    
});

module.exports = router;
