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

        function getAvailableCredits(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAvailableCredits' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
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