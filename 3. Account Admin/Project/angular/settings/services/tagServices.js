(function () {
    'use strict';

    var serviceId = 'tagServices';
    var urlBase = '/Tags/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', tagServices]);

    function tagServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getTags: getTags,

            // Create methods:
            createTag: createTag,

            // Delete methods:
            deleteTag: deleteTag,



        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getTags() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetTags' + cacheBuster });
        }


        /* ==========================================
               POST Methods
        ==========================================*/

        function createTag(tagName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateTag' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    tagName: tagName,
                }
            });
        }

        /* ==========================================
               POST Method (DELETE)
        ==========================================*/

        function deleteTag(tagName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteTag' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    tagName: tagName,
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