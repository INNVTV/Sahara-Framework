(function () {
    'use strict';

    var serviceId = 'paymentServices';
    var urlBase = '/Payments/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', paymentServices]);

    function paymentServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            getPayment: getPayment,

            getPaymentHistory: getPaymentHistory,
            getPaymentHistory_Next: getPaymentHistory_Next,
            getPaymentHistory_Last: getPaymentHistory_Last,

            refundPayment: refundPayment,

            getDunningAttempts: getDunningAttempts

        };

        return service;


        function getPayment(chargeId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPayment' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    chargeId: chargeId
                }
            });
        }

        function getPaymentHistory(itemLimit, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    accountId: accountId
                }
            });
        }

        function getPaymentHistory_Next(itemLimit, startingAfter, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startingAfter: startingAfter,
                    accountId: accountId
                }
            });
        }

        function getPaymentHistory_Last(itemLimit, endingBefore, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory_Last' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    endingBefore: endingBefore,
                    accountId: accountId
                }
            });
        }


        /* ==========================================
          REFUNDS
        ==========================================*/

        function refundPayment(accountId, chargeId, refundAmount) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/RefundPayment' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    chargeId: chargeId,
                    refundAmount: refundAmount
                }
            });
        }

        /* ==========================================
          DUNNING
        ==========================================*/

        function getDunningAttempts(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetDunningAttempts' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId
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