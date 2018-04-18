(function () {
    'use strict';

    var serviceId = 'billingServices';
    var urlBase = '/Billing/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', billingServices]);

    function billingServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Invoices -----------------------------------
            getNextInvoice: getNextInvoice,

            //Payments/Charges --------------------------

            getPayment: getPayment,

            getPaymentHistory: getPaymentHistory,
            getPaymentHistory_Next: getPaymentHistory_Next,
            getPaymentHistory_Last: getPaymentHistory_Last,

            //Dunning ------------------------------------
            getDunningAttempts: getDunningAttempts
        };

        return service;

        /* ==========================================
            INVOICES
        ==========================================*/

        function getNextInvoice() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetNextInvoice' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                }
            });
        }
        
        /* ==========================================
            PAYMENTS/CHARGES
        ==========================================*/

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

        function getPaymentHistory(itemLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                }
            });
        }

        function getPaymentHistory_Next(itemLimit, startingAfter) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startingAfter: startingAfter,
                }
            });
        }

        function getPaymentHistory_Last(itemLimit, endingBefore) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetPaymentHistory_Last' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    endingBefore: endingBefore,
                }
            });
        }

        /* ==========================================
            DUNNING
        ==========================================*/

        function getDunningAttempts() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetDunningAttempts' + cacheBuster,
                transformRequest: transformRequest,
                params: {
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