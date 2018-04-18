(function () {
    'use strict';

    var serviceId = 'accountServices';
    var urlBase = '/account/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountServices]);

    function accountServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Credit Cards -----------------------------
            getCardInfo: getCardInfo,
            addUpdateCard: addUpdateCard,

            // Get methods:
            getAccount: getAccount,
            getAccountByIdForSub: getAccountByIdForSub, //<-- Only used for sub/ controller
            getAccountImages: getAccountImages,

            //Logs -----------------------------
            getAccountLog: getAccountLog,
            getAccountLogByCategory: getAccountLogByCategory,
            getAccountLogByActivity: getAccountLogByActivity,
            getAccountLogByUser: getAccountLogByUser,
            getAccountLogByObject: getAccountLogByObject,
            //Log Types
            getAccountLogCategories: getAccountLogCategories,
            getAccountLogActivities: getAccountLogActivities,

            //Accounts Lists (for trade & communication purposes)
            getAccountsList: getAccountsList,

            //Account Capacity 
            getAccountCapacity: getAccountCapacity,

            //Close account
            closeAccount: closeAccount,

        };

        return service;

        /* ===============================================================

         ACCOUNT

       ================================================================*/

        /* ===========  GET  ==========================================*/

        function getAccount() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccount' + cacheBuster });
        }

        function getAccountByIdForSub(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetAccountByIdForSub' + cacheBuster,
                    params: {
                        accountId: accountId,
                    }
            });
        }

        function getAccountImages() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountImages' + cacheBuster });
        }


        function getAccountCapacity() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountCapacity' + cacheBuster });
        }

        /* ===========  CLOSE  ==========================================*/

        function closeAccount() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/CloseAccount' + cacheBuster });
        }

        /* ===============================================================

          CARDS

        ================================================================*/

        /* ===========  GET  ==========================================*/

        function getCardInfo() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCardInfo' + cacheBuster });
        }

        /* ===========  POST  ==========================================*/

        function addUpdateCard(cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/AddUpdateCard' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    cardName: cardName,
                    cardNumber: cardNumber,
                    cvc: cvc,
                    expirationMonth: expirationMonth,
                    expirationYear: expirationYear,
                }
            });
        }


        /* =========================================
            LOGS
        ===========================================*/

        function getAccountLog(maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLog' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByCategory(categoryType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByCategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryType: categoryType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByActivity(activityType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByActivity' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    activityType: activityType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByUser(userId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByObject(objectId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByObject' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectId: objectId,
                    maxRecords: maxRecords,
                }
            });
        }

        /* ==========================================
         LOG TYPES
       ==========================================*/

        function getAccountLogCategories() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountLogCategories' + cacheBuster });
        }

        function getAccountLogActivities() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountLogActivities' + cacheBuster });
        }

        /* =========================================
           ACCOUNTS LISTS
        ===========================================*/

        function getAccountsList() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountsList' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    //maxRecords: maxRecords,
                }
            });
        }


        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce
            headers['Content-Type'] = 'application/json';
        };

    }
})();