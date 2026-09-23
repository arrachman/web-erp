const jwt = require('jsonwebtoken');
require('dotenv').config();

const secretKey = process.env.JWT_SECRET || 'your-secret-key';

const signToken = (payload, options = {}) => {
    return jwt.sign(payload, secretKey, { expiresIn: '1h', ...options });
};

const verifyToken = (token) => {
    try {
        return jwt.verify(token, secretKey);
    } catch (err) {
        throw new Error('Invalid or expired token');
    }
};

module.exports = {
    signToken,
    verifyToken
};