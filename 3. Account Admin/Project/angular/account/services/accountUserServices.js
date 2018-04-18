(function () {
    'use strict';

    var serviceId = 'accountUserServices';
    var urlBase = '/account/Json/Users';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountUserServices]);

    function accountUserServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //users -----------------------------------
            getUsers: getUsers,

            inviteUser: inviteUser,
            deleteUser: deleteUser,

            sendPasswordLink: sendPasswordLink,
            updateUserName: updateUserName,
            updateUserEmail: updateUserEmail,
            updateUserRole: updateUserRole,
            changeUserActiveState: changeUserActiveState,
            changeUserOwnershipStatus: changeUserOwnershipStatus,

            //invitations -----------------------------
            getInvitations: getInvitations,
            deleteInvitation: deleteInvitation,
            resendInvitation: resendInvitation,

        };

        return service;

        /* ==========================================
            USERS
        ==========================================*/

        function getUsers() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetUsers' + cacheBuster });
        }

        function deleteUser(userId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                }
            });
        }

        function updateUserName(userId, firstName, lastName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    firstName: firstName,
                    lastName: lastName
                }
            });
        }

        function updateUserEmail(userId, email) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    email: email
                }
            });
        }

        function updateUserRole(userId, role) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateRole' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    role: role
                }
            });
        }

        function changeUserActiveState(userId, isActive) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ChangeActiveState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    isActive: isActive
                }
            });
        }


        function changeUserOwnershipStatus(userId, isOwner) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ChangeOwnerStatus' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    isOwner: isOwner
                }
            });
        }


        function sendPasswordLink(email) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SendPasswordLink' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    email: email
                }
            });
        }


        /* ==========================================
          INVITATIONS
        ==========================================*/

        function getInvitations() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetInvitations' + cacheBuster });
        }

        function inviteUser(email, firstName, lastName, role, isOwner) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/InviteUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    role: role,
                    isOwner: isOwner,
                }
            });
        }

        function deleteInvitation(invitationKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteInvitation' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    invitationKey: invitationKey,
                }
            });
        }

        function resendInvitation(invitationKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResendInvitation' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    invitationKey: invitationKey,
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