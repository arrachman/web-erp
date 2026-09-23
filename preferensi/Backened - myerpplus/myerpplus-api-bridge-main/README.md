## Modules
We have base route for each module:
- m1: /api/master-data
- m2: /api/finance
- m3: /api/inventory
- m4: /api/purchasing
- m5: /api/sales

## API configuration terms
For API configuration, please refer to api-config directory. Here is the terms of json API configuration 
- `method`: Rest API method (POST, PUT, PATCH, DELETE, GET),
- `route`: routing url untuk API
- `package`: MyERPPlus WS package
- `route`: url routing. For example if you want to config for inventory API and you set route `/transfer-stock`, you can access your API with: http://myerpplus.com/api/inventory/transfer-stock
- `isUpdate`: please use for update the data(Update status, update transaction, update master data, etc)
- `requestBody`: request configuration. please put the right params for request body
    - `order`: The order mapping for request to MyERPPlus WS
    - `otherParams`: this params will handle other `detail` request. if you have request with sptSubParam, please put here
    - inside of otherParams, we have key `params`. You can put your additional params such as detail, serial, batch, and etc params there
- `responseBodyParams`: response configuration.
    - `otherData`: this params will handle other `detail` request. if you have response with sptSubParam, please put here
    - `order`: The order mapping for get response field from MyERPPlus WS Data

We have 2 types of responseBodyParams:
- `array` : data format will be like this []
- `object` : data format will be like this {}

We have several special default value on requsestBody:
- `date-time-now`: value will be date of today
- `userId`: value will be userId from userlogin
- `id`: default value will be transactionId
- `app-key`: default app key that stored in env variable APP_KEY
- `secret-key`: default secret key that stored in env variable SECRET_KEY
- `app-code`: default app code that the value stored in APP_CODE
  
## Installation
How to run this app:
Just follow this instruction https://medium.com/@adarsh-d/deploy-node-js-application-on-iis-9703d5dfcaca


## Postman Collection Example
https://api.postman.com/collections/14623147-b04b4349-9738-4e7a-993f-a6a10578c11d?access_key=PMAT-01J7QXSS900NN7VJT51E30MYKV