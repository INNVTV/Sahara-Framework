(function () {
    'use strict';

    var serviceId = 'commerceServices';
    var urlBase = '/Commerce/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', sharedServices]);

    function sharedServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Credits -----------------------------
            getCreditsToDollarExchangeValue: getCreditsToDollarExchangeValue,
            getAvailableCredits: getAvailableCredits,
            //getCreditsTransactionLog: getCreditsTransactionLog,
            buyCredits: buyCredits,
            spendCredits: spendCredits,
            tradeCredits: tradeCredits,


        };

        return service;

        /* ===============================================================

          CREDITS

        ================================================================*/

        /* ===========  GET  ==========================================*/

        function getCreditsToDollarExchangeValue() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCreditsToDollarExchangeValue/' + cacheBuster });
        }

        function getAvailableCredits() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAvailableCredits/' + cacheBuster });
        }

        /*
        function getCreditsTransactionLog() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCreditsTransactionLog/' + cacheBuster });
        }*/

        /* ===========  BUY  ==========================================*/

        function buyCredits(dollarAmount) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/BuyCredits' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    dollarAmount: dollarAmount,
                }
            });

        }

        /* ===========  SPEND  ==========================================*/

        function spendCredits(creditsAmount, description) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SpendCredits' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    creditsAmount: creditsAmount,
                    description: description,
                }
            });
        }


        /* ===========  TRADE  ==========================================*/

        function tradeCredits(toAccountId, creditsAmount, description) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/TradeCredits' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    toAccountId : toAccountId,
                    creditsAmount: creditsAmount,
                    description: description,
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