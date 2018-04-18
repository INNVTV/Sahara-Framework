(function () {
    'use strict';

    var serviceId = 'leadsServices';
    var urlBase = '/Leads/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', leadsServices]);

    function leadsServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Get methods
            getSalesLeads: getSalesLeads,

            //Move methods (Also used to delete)
            moveSalesLead: moveSalesLead,

            //Update methods
            updateSalesLead: updateSalesLead,

            //Submit methods
            submitSalesLead: submitSalesLead,

            // Settings methods
            addSalesLeadLabel: addSalesLeadLabel,
            removeSalesLeadLabel: removeSalesLeadLabel,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getSalesLeads(label, take, lastPartitionKey, lastRowKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/GetSalesLeads' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    label: label,
                    take: take,
                    lastPartitionKey: lastPartitionKey,
                    lastRowKey: lastRowKey,
                }
            });
        }

        /* ==========================================
               MOVE Methods
        ==========================================*/

        function moveSalesLead(partitionKey, rowKey, fromLabel, toLabel) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/MoveSalesLead' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    partitionKey:partitionKey,
                    rowKey:rowKey,
                    fromLabel:fromLabel,
                    toLabel:toLabel
                }
            });
        }

        /* ==========================================
               UPDATE Methods
        ==========================================*/

        function updateSalesLead(partitionKey, rowKey, label, field, value) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSalesLead' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    partitionKey: partitionKey,
                    rowKey: rowKey,
                    label: label,
                    field: field,
                    value: value
                }
            });
        }

        /* ==========================================
               SUBMIT Methods
        ==========================================*/

        function submitSalesLead(label, firstName, lastName, companyName, phone, email, comments, notes, productName, productId, fullyQualifiedName, locationPath, origin, ipAddress, userId, userName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SubmitSalesLead' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    label: label,
                    firstName: firstName,
                    lastName: lastName,
                    companyName: companyName,
                    phone: phone,
                    email: email,
                    comments: comments,
                    notes: notes,
                    productName: productName,
                    productId: productId,
                    fullyQualifiedName: fullyQualifiedName,
                    locationPath: locationPath,
                    origin: origin,
                    ipAddress: ipAddress,
                    userId: userId,
                    userName: userName
                }
            });
        }

       

        /* ==========================================
               SETTINGS Methods
        ==========================================*/

        function addSalesLeadLabel(label) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/AddSalesLeadLabel' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    label: label,
                }
            });
        }

        function removeSalesLeadLabel(index) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/RemoveSalesLeadLabel' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    index: index,
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