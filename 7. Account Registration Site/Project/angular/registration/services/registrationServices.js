(function () {
    'use strict';

    var serviceId = 'registrationServices';
    //var urlBase = '/Registration/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', registrationServices]);

    function registrationServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // validate inputs:
            validateAccountName: validateAccountName,
            validateFirstName: validateFirstName,
            validateLastName: validateLastName,
            validateEmail: validateEmail,
            validatePhoneNumber: validatePhoneNumber,
            //validatePassword: validatePassword,
            //validateConfirmationPassword: validateConfirmationPassword,

            // Submit Registration
            submitRegistration: submitRegistration,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        //function validateAccountName() {

        //var date = new Date();
        //var cacheBuster = "?cb=" + date.getTime();

        //return $http({ method: 'POST', url: registrationEndpoint + '/validation/accountname?dt=' + cacheBuster });
        // }


        /* ==========================================
               Validation Methods
        ==========================================*/


        function validateAccountName(registrationEndpoint, accountname) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/validation/accountname' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"accountname":"' + accountname + '"}'

            });
        };

        function validateFirstName(registrationEndpoint, firstname) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/validation/firstname' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"firstName":"' + firstname + '"}'

            });
        };

        function validateLastName(registrationEndpoint, lastname) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/validation/lastname' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"lastName":"' + lastname + '"}'

            });
        };

        function validateEmail(registrationEndpoint, email) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/validation/email' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"email":"' + email + '"}'

            });
        };


        function validatePhoneNumber(registrationEndpoint, phoneNumber) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/validation/phonenumber' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"phonenumber":"' + phoneNumber + '"}'

            });
        };

        /* ==========================================
           Registration Method
        ==========================================*/


        function submitRegistration(registrationEndpoint, accountName, firstName, lastName, email, phoneNumber, password, confirmPassword, origin) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: registrationEndpoint + '/register' + cacheBuster,
                transformRequest: transformRequest,
                data: '{"accountname":"' + accountName + '", ' +
                       '"email":"' + email + '", ' +
                       '"phonenumber":"' + phoneNumber + '", ' +
                       '"firstname":"' + firstName + '", ' +
                       '"lastname":"' + lastName + '", ' +
                       '"password":"' + password + '", ' +
                       '"confirmpassword":"' + confirmPassword + '", ' +
                       '"origin":"' + origin + '"' +
                       '}'
            });
        };

        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce;
            headers['Content-Type'] = 'application/x-www-form-urlencoded';
        };

    };
})();