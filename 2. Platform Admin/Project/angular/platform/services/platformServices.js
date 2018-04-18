(function () {
    'use strict';

    var serviceId = 'platformServices';
    var urlBase = '/platform/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', platformServices]);

    function platformServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //users -----------------------------------
            getUsers: getUsers,

            inviteUser: inviteUser,
            createUser: createUser,
            deleteUser: deleteUser,

            sendPasswordLink: sendPasswordLink,
            updateUserName: updateUserName,
            updateUserEmail: updateUserEmail,
            updateUserRole: updateUserRole,
            changeUserActiveState: changeUserActiveState,

            //invitations -----------------------------
            getInvitations: getInvitations,
            deleteInvitation: deleteInvitation,
            resendInvitation: resendInvitation,

            //Password -----------------------------
            getPasswordClaims: getPasswordClaims,

            //Snapshot ----------------------------
            getSnapshot: getSnapshot,

            //DocumentPartitions & Tiers -----------------------------

            //getDocumentPartitions: getDocumentPartitions,
            //getDocumentPartition: getDocumentPartition,

            getDocumentPartitionCollectionProperties: getDocumentPartitionCollectionProperties,
            getDocumentPartitionTenantCollectionProperties: getDocumentPartitionTenantCollectionProperties,

            //SQL Partitions -----------------------------
            getSqlPartitions: getSqlPartitions,
            getSqlPartition: getSqlPartition,
            getSqlPartitionSchemas: getSqlPartitionSchemas,
            getSqlPartitionTenantSchemaLog: getSqlPartitionTenantSchemaLog,

            //Search Partitions -----------------------------
            getSearchPartitions: getSearchPartitions,

            //Storage Partitions -----------------------------
            getStoragePartitions: getStoragePartitions,

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


        function createUser(email, firstName, lastName, role, password, confirmPassword) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    role: role,
                    password: password,
                    confirmPassword: confirmPassword,
                }
            });
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

        function inviteUser(email, firstName, lastName, role) {

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
          PASSWORD CLAIMS
        ==========================================*/

        function getPasswordClaims() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPasswordClaims' + cacheBuster });
        }

        /* ==========================================
          SNAPSHOT
        ==========================================*/

        function getSnapshot() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSnapshot' + cacheBuster });
        }

        /* ==========================================
          DOCUMENT PARTITIONS
        ==========================================*/
        /*
        function getDocumentPartitions(documentPartitionTierId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetDocumentPartitions?documentPartitionTierId=' + documentPartitionTierId + cacheBuster });
        }

        function getDocumentPartition(documentPartitionId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetDocumentPartition?documentPartitionId=' + documentPartitionId + cacheBuster });
        }*/

        function getDocumentPartitionCollectionProperties(documentPartitionId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetDocumentPartitionCollectionProperties?documentPartitionId=' + documentPartitionId + cacheBuster });
        }

        function getDocumentPartitionTenantCollectionProperties(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetDocumentPartitionTenantCollectionProperties?accountId=' + accountId + cacheBuster });
        }


        /* ==========================================
          SQL PARTITIONS
        ==========================================*/

        function getSqlPartitions() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSqlPartitions?' + cacheBuster });
        }

        function getSqlPartition(sqlPartitionName) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSqlPartition?sqlPartitionName=' + sqlPartitionName + cacheBuster });
        }

        function getSqlPartitionSchemas(sqlPartitionName) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSqlPartitionSchemas?sqlPartitionName=' + sqlPartitionName + cacheBuster });
        }

        function getSqlPartitionTenantSchemaLog(accountId)
        {
            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSqlPartitionTenantSchemaLog?accountId=' + accountId + cacheBuster });
        }

        /* ==========================================
          SEARCH PARTITIONS
        ==========================================*/

        function getSearchPartitions() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSearchPartitions?' + cacheBuster });
        }

        /* ==========================================
          STORAGE PARTITIONS
        ==========================================*/

        function getStoragePartitions() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetStoragePartitions?' + cacheBuster });
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