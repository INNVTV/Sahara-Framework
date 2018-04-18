(function () {
    'use strict';

    var serviceId = 'invoiceServices';
    var urlBase = '/Invoices/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', invoiceServices]);

    function invoiceServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            getInvoice: getInvoice,

            getNextInvoice: getNextInvoice,
            getInvoiceHistory: getInvoiceHistory,
            getInvoiceHistory_Next: getInvoiceHistory_Next,
            getInvoiceHistory_Last: getInvoiceHistory_Last,

            getInvoiceHistory_ByDateRange: getInvoiceHistory_ByDateRange,
            getInvoiceHistory_ByDateRange_Next: getInvoiceHistory_ByDateRange_Next,
            getInvoiceHistory_ByDateRange_Last: getInvoiceHistory_ByDateRange_Last,

        };

        return service;

        function getInvoice(invoiceId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoice' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    invoiceId: invoiceId
                }
            });
        }

        function getNextInvoice(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetNextInvoice' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId
                }
            });
        }

        function getInvoiceHistory(itemLimit, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    accountId: accountId
                }
            });
        }

        function getInvoiceHistory_Next(itemLimit, startingAfter, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startingAfter: startingAfter,
                    accountId: accountId
                }
            });
        }

        function getInvoiceHistory_Last(itemLimit, endingBefore, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory_Last' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    endingBefore: endingBefore,
                    accountId: accountId
                }
            });
        }

        // ---------------------------------------------

        function getInvoiceHistory_ByDateRange(itemLimit, startDate, endDate, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory_ByDateRange' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate,
                    accountId: accountId
                }
            });
        }

        function getInvoiceHistory_ByDateRange_Next(itemLimit, startDate, endDate, startingAfter, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory_ByDateRange_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate,
                    startingAfter: startingAfter,
                    accountId: accountId
                }
            });
        }

        function getInvoiceHistory_ByDateRange_Last(itemLimit, startDate, endDate, endingBefore, accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetInvoiceHistory_ByDateRange_Last'  + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate,
                    endingBefore: endingBefore,
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