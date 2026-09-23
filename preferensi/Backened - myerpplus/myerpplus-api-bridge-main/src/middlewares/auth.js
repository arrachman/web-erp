const { verifyToken } = require('../utils/jwt');

const authenticateJWT = (req, res, next) => {
    const authHeader = req.headers.authorization;

    if (!authHeader) {
        return res.status(401).json({ error: 'Authorization header missing' });
    }

    try {
        const token = req.headers.authorization.split(' ')[1];
        const decoded = verifyToken(token);
        accessKey = decoded["accessKey"]
        userId = decoded["userid"]

        req.user = {
            accessKey,
            userId
        }
        next();  // Proceed to the next middleware or route handler
    } catch (err) {
        return res.status(401).json({ error: 'Unauthorized' });
    }
};

module.exports = authenticateJWT;
