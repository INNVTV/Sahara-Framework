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

            // Get methods:
            getSnapshot: getSnapshot,
            getReport: getReport,
            getBalanceTransactionsForSource: getBalanceTransactionsForSource
        };

        return service;

        /* ==========================================
          SNAPSHOT
        ==========================================*/

        function getSnapshot() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSnapshot' + cacheBuster });
        }

        /* ==========================================
          REPORT
        ==========================================*/

        function getReport(sinceHoursAgo) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetReport' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    sinceHoursAgo: sinceHoursAgo
                }
            });
        }

        /* ==========================================
          BALANCE TRANSACTION
        ==========================================*/

        function getBalanceTransactionsForSource(sourceId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetBalanceTransactionsForSource' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    sourceId: sourceId
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