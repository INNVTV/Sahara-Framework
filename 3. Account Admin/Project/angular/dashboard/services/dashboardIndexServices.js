(function () {
    'use strict';

    var serviceId = 'dashboardIndexServices';
    var urlBase = '/Dashboard/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', dashboardIndexServices]);

    function dashboardIndexServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getData: getData,

            // Update methods
            updateData: updateData,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getData() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Count' + cacheBuster });
        }


        /* ==========================================
               POST Methods
        ==========================================*/

        function updateData(id, name) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    id: id,
                    name: name
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