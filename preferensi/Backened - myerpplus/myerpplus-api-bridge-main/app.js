const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const authenticateJWT = require('./src/middlewares/auth');
require('dotenv').config();

const app = express();

// Middleware
app.use(cors());
app.use(express.json());
app.use(morgan('dev'));

// Routes
app.get('/', async (req, res) => {
    res.json("Welcome to Public MyERPPlus API");
}); 
app.use('/api', require('./src/router/non-authorized-route'));
app.use('/api/master-data', authenticateJWT,  require('./src/router/master-data-route'))
app.use('/api/finance', authenticateJWT,  require('./src/router/finance-route'))
app.use('/api/inventory', authenticateJWT,  require('./src/router/inventory-route'))
app.use('/api/purchasing', authenticateJWT,  require('./src/router/purchasing-route'))
app.use('/api/sales', authenticateJWT,  require('./src/router/sales-route'))
app.use('/api/production', authenticateJWT,  require('./src/router/production-route'))

// Error handling middleware
app.use((err, req, res, next) => {
    console.error(err.stack);
    res.status(500).send('Something broke!');
});

module.exports = app;
