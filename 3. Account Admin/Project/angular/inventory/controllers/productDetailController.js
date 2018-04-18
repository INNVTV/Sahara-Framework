(function () {
    'use strict';

    var controllerId = 'productDetailController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'productServices',
            'sharedServices',
            'categoryServices',
            'accountServices',
            'tagServices',
            'propertiesServices',
            'imageServices',
            'accountSettingsServices',
            'leadsServices',
            '$routeParams',
            '$location',
             productDetailController
    ]);

    function productDetailController(productServices, sharedServices, categoryServices, accountServices, tagServices, propertiesServices, imageServices, accountSettingsServices, leadsServices, $routeParams, $location) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'productIndexController';
        vm.activate = activate;
        vm.headerBGColor = "#F5F5F5";

        vm.category = null
        vm.categoryNameKey = null;
        vm.categoryName = null;

        vm.subcategory = null
        vm.subcategoryNameKey = null;
        vm.subcategoryName = null;

        vm.subsubcategory = null
        vm.subsubcategoryNameKey = null;
        vm.subsubcategoryName = null;

        vm.subsubsubcategory = null
        vm.subsubsubcategoryNameKey = null;
        vm.subsubsubcategoryName = null;

        vm.productNameKey = null;

        //Route Properties =============================
        vm.fullyQualifiedPath = "";

        if ($routeParams.categoryNameKey)
        {
            vm.fullyQualifiedPath += $routeParams.categoryNameKey
            vm.categoryNameKey = $routeParams.categoryNameKey;
        }
        if ($routeParams.subcategoryNameKey) {
            vm.fullyQualifiedPath += "/" + $routeParams.subcategoryNameKey
            vm.subcategoryNameKey = $routeParams.subcategoryNameKey;
        }
        if ($routeParams.subsubcategoryNameKey) {
            vm.fullyQualifiedPath += "/" + $routeParams.subsubcategoryNameKey
            vm.subsubcategoryNameKey = $routeParams.subsubcategoryNameKey;
        }
        if ($routeParams.subsubsubcategoryNameKey) {
            vm.fullyQualifiedPath += "/" + $routeParams.subsubsubcategoryNameKey
            vm.subsubsubcategoryNameKey = $routeParams.subsubsubcategoryNameKey;
        }
        if ($routeParams.productNameKey) {
            vm.fullyQualifiedPath += "/" + $routeParams.productNameKey
            vm.productNameKey = $routeParams.productNameKey;
        }


        /* ==========================================
               Helper Methods
        ==========================================*/

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };

        //Only allow Google Maps to initiate ONCE per controller creation!
        var mapsInitiated = false;

        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        /* ==========================================
                Controller Properties
        ==========================================*/


        





        /* ==========================================
           PRODUCT NAME
        ==========================================*/

        vm.nameStatus =
            {
                Updating: false,
                Editing: false,
                SendingComplete: false,

                Name: null,

                Results: {
                    IsSuccess: false,
                    Message: null
                },
                Retry: function () {
                    this.Updating = false;
                    this.Editing = true;
                    this.SendingComplete = false;
                },

                Clear: function () {

                    //Debug.trace("Clearing update product name form data.");
                    this.Updating = false;
                    this.Editing = false;
                    this.SendingComplete = false;
                }


            }

        vm.editName = function () {
            //Debug.trace("Editing product name...");

            vm.nameStatus.NewName = vm.product.Name;

            vm.nameStatus.Editing = true;

        }

        vm.cancelUpdateName = function () {

            vm.nameStatus.Clear();

        }

        vm.updateName = function () {
            //Debug.trace("Updating product name...");

            vm.nameStatus.Updating = true;
            vm.nameStatus.Editing = false;

            productServices.updateName(vm.product.FullyQualifiedName, vm.product.Name, vm.nameStatus.NewName)
           .success(function (data, status, headers, config) {

               vm.nameStatus.Updating = false;
               vm.nameStatus.SendingComplete = true;

               if (data.isSuccess) {

                   vm.nameStatus.Results.IsSuccess = true;
                   vm.nameStatus.Results.Message = "Product name has been changed.";
                   vm.nameStatus.Clear();

                   //Update ProductName and ProductNameKey 
                   vm.product.Name = vm.nameStatus.NewName;
                   vm.product.NameKey = data.Results[0];
                   vm.product.FullyQualifiedName = vm.product.LocationPath + "/" + data.Results[0];

                   vm.fullyQualifiedPath = vm.product.FullyQualifiedName;

                   //Update URL path
                   $location.update_path('/item/' + vm.product.FullyQualifiedName, true);
       
                   //setTimeout(function () {
                      
                   //}, .5);
                   
                   
               }
               else {
                   vm.nameStatus.Results.IsSuccess = false;
                   vm.nameStatus.Results.Message = data.ErrorMessage;

               }

           }).error(function (data, status, headers, config) {

               vm.nameStatus.IsSuccess = false;
               vm.nameStatus.Results.Message = "An error has occurred while using the service.";
           })

        }

        vm.cancelNameEdit = function () {

            vm.nameStatus.Editing = false;
            vm.nameStatus.Updating = false;
        }

        //vm.refresh = function () {

           //vm.nameStatus.Clear();

            //vm.getAccount();

            //vm.showGetAccountLoading = true;

            //var newUrl = 'product/' + vm.product.NameKey;
            //vm.product = null;
            /////////////////////////////window.location.href = newUrl;
            //window.location.replace(newUrl);


            //if (typeof (history.pushState) != "undefined") {
                //var obj = { Page: 'page', Url: newUrl };
                //alert("State: " + history.pushState);
                //history.pushState(obj, obj.Page, obj.Url);
                //history.pushState(null, null, vm.product.NameKey);
                //history.pushState({ id: 'SOME ID' }, '', vm.product.NameKey);
                //$stateProvider.state('ctrl', {
                   // url: vm.product.NameKey,
                    //template: '<ui-view autoscroll="autoscroll"/>'
                //});

                //$scope.autoscroll = false;
            //} else {
                //alert("Browser does not support HTML5.");
            //}

        //}

        /* ==========================================
           END CATEGORY NAME
        ==========================================*/








        /* =========================================

            CHANGE VISIBILITY

            ========================================*/

        vm.makeVisible = function()
        {
            vm.product.Visible = null;

            productServices.updateProductVisibleState(vm.product.FullyQualifiedName, vm.product.Name, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.product.Visible = true;
                }
                else {
                    vm.product.Visible = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.product.Visible = false;
                })
        }

        vm.makeHidden = function () {
            vm.product.Visible = null;

            productServices.updateProductVisibleState(vm.product.FullyQualifiedName, vm.product.Name, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.product.Visible = false;
                }
                else {
                    vm.product.Visible = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.product.Visible = true;
                })
        }





        /* ==========================================

          GET PRODUCT LOG

       ==========================================*/

        vm.productLog = null;

        vm.getProductLog = function () {

            vm.productLog = null;

            accountServices.getAccountLogByObject(vm.product.ProductID, 50)
            .success(function (data, status, headers, config) {

                vm.productLog = data;
            })
            .error(function (data, status, headers, config) {

            })

        }


        /* ==========================================

         END GET PRODUCT LOG

       ==========================================*/











        /*===========================================
          
                PROPERTY METHODS
          
         ===========================================*/

        //vm.globalPropertyEditingState = false //<-- Used to hide edit buttons of other properties when we are in edit mode for any single property

        vm.properties = null;

        vm.getProductProperties = function () {

            //Debug.trace("Getting product properties...");

            productServices.getProductProperties(vm.product.FullyQualifiedName)
            .success(function (data, status, headers, config) {

                vm.productProperties = data;
                //vm.initiatePropertyUI();
                //vm.propertyDetail = vm.properties[currentPropertiesIndex];

                var locationDataExists = false;
                
                //loop through types to determine if any location options exist
                for (var t = 0; t < vm.productProperties.length; t++) {

                    if (vm.productProperties[t].ValueType == "location")
                    {
                        locationDataExists = true;
                    }
                }

                if (locationDataExists) {
                    //Initiate Google Maps JS API if location data exists
                    if (!mapsInitiated)
                    {
                        vm.initGoogleMaps();
                    }

                    mapsInitiated = true;
                }


            })
            .error(function (data, status, headers, config) {

            })
        }


        /*===========================================         
                INITIATE PROPERTIES UI       
         ===========================================*/

        /*
        vm.initiatePropertyUI = function()
        {
            //loop through types and initiate relevant ui controls
            for (var t = 0; t < vm.productProperties.length; t++) {

                if (vm.productProperties[t].ValueType == "datetime")
                {
                    if (vm.productProperties[t].AssignedValue == null || vm.productProperties[t].AssignedValue == '')
                    {
                        //if empty use current date/time
                        $('#date-picker-' + vm.productProperties[t].NameKey).combodate({ value: new Date() });
                    }
                    else{
                        //Otherwise use the assigned value
                        $('#date-picker-' + vm.productProperties[t].NameKey).combodate({ value: new Date(vm.productProperties[t].AssignedValue) });
                    }                  
                }
            }           
        }*/


        /*===========================================         
                EDIT PROPERTIES       
         ===========================================*/

        vm.propertyDetails =
             {
                 CurrentIndex: null,
                 PropertyName: null,
                 Appendable: null,

                 AssignedValue: null,
                 AssignedValues: [],
                 AssignedSwatches: [],
                 PredefinedValues: [],

                 Symbol: null,
                 SymbolPlacement: null,

                 PreviousValue: null,
                 PreviousValues: [],
                 ValueToReplaceOrAppend: null,
                 PredefinedValuesDiff:[],
                 AvailableSwatchesDiff: [],
                 ValueType: null,

                 ShowOptions: false,

                 // Location Data --------

                 LocationName: null,
                 Address1: null,
                 Address2: null,
                 City: null,
                 State: null,
                 PostalCode: null,
                 Country: null,
                 Lat: null,
                 Long: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },
                 UpdateValue: function(value)
                 {
                     this.ValueToReplaceOrAppend = value;                   
                 },
                 ShowMoreOptions: function () {
                     this.ShowOptions = true;
                 },
                 HideMoreOptions: function (value) {
                     this.ShowOptions = false;
                 },
                 Clear: function () {

                     this.CurrentIndex = null;
                     this.Appendable = null;

                     this.AssignedValue = null;
                     this.AssignedSwatches = [];
                     this.AssignedValues = [];
                     this.PredefinedValues = [];

                     this.Symbol = null;
                     this.SymbolPlacement = null;

                     this.PropertyName = null;
                     this.ValueToReplaceOrAppend = null;
                     this.PredefinedValuesDiff = [];
                     this.AvailableSwatchesDiff = [];
                     //this.ValueToReplaceOrAppends = [];
                     this.PreviousValue = null;
                     this.PreviousValues = [];
                     this.ValueType = null;


                     this.LocationName = null;
                     this.Address1 = null;
                     this.Address2 = null;
                     this.City = null;
                     this.State = null;
                     this.PostalCode = null;
                     this.Country = null;
                     this.Lat = null;
                     this.Long = null;

                     this.ShowOptions = false;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.editProperty = function(index)
        {
            vm.editLatLong = false; //<-- Clear editing of Lat/Longs if location property

            //vm.initiatePropertyUI(); //<-- For calandar & map controls

            vm.propertyDetails.Clear();
            //vm.globalPropertyEditingState = true;

            vm.propertyDetails.CurrentIndex = index;
            vm.propertyDetails.ValueType = vm.productProperties[index].ValueType;
            vm.propertyDetails.Appendable = vm.productProperties[index].Appendable;
            vm.propertyDetails.PropertyName = vm.productProperties[index].Name;

            vm.propertyDetails.AssignedValue = vm.productProperties[index].AssignedValue;
            vm.propertyDetails.AssignedValues = vm.productProperties[index].AssignedValues;
            vm.propertyDetails.PredefinedValues = vm.productProperties[index].PredefinedValues;
            vm.propertyDetails.AssignedSwatches = vm.productProperties[index].AssignedSwatches;

            vm.propertyDetails.Symbol = vm.productProperties[index].Symbol;
            vm.propertyDetails.SymbolPlacement = vm.productProperties[index].SymbolPlacement;

            if (vm.productProperties[index].ValueType == "predefined")
            {
                vm.propertyDetails.PreviousValues = vm.productProperties[index].AssignedValues;

                if (vm.productProperties[index].Appendable == true)
                {
                    //Debug.trace(vm.productProperties[index].PredefinedValues);
                   // vm.propertyDetails.PredefinedValuesDiff = vm.productProperties[index].PredefinedValues.slice();

                    //Pipe in values and rename to generic items, ignore ones that are already assigned----------------
                    for (var p = 0, len = vm.productProperties[index].PredefinedValues.length; p < len; ++p) {

                        var exists = false;

                        if (vm.productProperties[index].AssignedValues != null)
                        {
                            for (var a = 0, len2 = vm.productProperties[index].AssignedValues.length; a < len2; ++a) {
                                //Debug.trace(vm.productProperties[index].AssignedValues[a] + "==" + vm.productProperties[index].PredefinedValues[p])
                                if (vm.productProperties[index].AssignedValues[a] == vm.productProperties[index].PredefinedValues[p])
                                {
                                    exists = true;
                                }
                            };
                        }
                        
                        if (!exists) {
                            vm.propertyDetails.PredefinedValuesDiff.push(vm.productProperties[index].PredefinedValues[p]);
                        }
                    };
                }
            }
            else if (vm.productProperties[index].ValueType == "swatch")
            {
                vm.propertyDetails.PreviousValues = vm.productProperties[index].AssignedSwatches;

                //if (vm.productProperties[index].Appendable == true) {

                    //Pipe in values and rename to generic items, ignore ones that are already assigned----------------
                    for (var p = 0, len = vm.productProperties[index].AvailableSwatches.length; p < len; ++p) {

                        var exists = false;

                        if (vm.productProperties[index].AssignedSwatches != null) {
                            for (var a = 0, len2 = vm.productProperties[index].AssignedSwatches.length; a < len2; ++a) {
                                //Debug.trace(vm.productProperties[index].AssignedSwatches[a] + "==" + vm.productProperties[index].AvailableSwatches[p])
                                if (vm.productProperties[index].AssignedSwatches[a].Label == vm.productProperties[index].AvailableSwatches[p].Label) {
                                    exists = true;
                                }
                            };
                        }

                        if (!exists) {
                            vm.propertyDetails.AvailableSwatchesDiff.push(vm.productProperties[index].AvailableSwatches[p]);
                        }
                    };
                //}

            }
            else if (vm.productProperties[index].ValueType == "string" || vm.productProperties[index].ValueType == "number" || vm.productProperties[index].ValueType == "paragraph" || vm.productProperties[index].ValueType == "datetime")
            {
                vm.propertyDetails.ValueToReplaceOrAppend = vm.productProperties[index].AssignedValue;
                vm.propertyDetails.PreviousValue = vm.productProperties[index].AssignedValue;
            }
            else if (vm.productProperties[index].ValueType == "location")
            {
                if (vm.productProperties[index].LocationData != null)
                {
                    vm.propertyDetails.LocationName = vm.productProperties[index].LocationData.Name;
                    vm.propertyDetails.Address1 = vm.productProperties[index].LocationData.Address1;
                    vm.propertyDetails.Address2 = vm.productProperties[index].LocationData.Address2;
                    vm.propertyDetails.City = vm.productProperties[index].LocationData.City;
                    vm.propertyDetails.State = vm.productProperties[index].LocationData.State;
                    vm.propertyDetails.PostalCode = vm.productProperties[index].LocationData.PostalCode;
                    vm.propertyDetails.Country = vm.productProperties[index].LocationData.Country;
                    vm.propertyDetails.Lat = vm.productProperties[index].LocationData.Lat;
                    vm.propertyDetails.Long = vm.productProperties[index].LocationData.Long;
                }

                if (vm.propertyDetails.Lat != null && vm.propertyDetails.Long != null)
                {
                    setTimeout(function () { vm.initLocationData(vm.propertyDetails.Lat, vm.propertyDetails.Long, false); }, 300);
                }
                else {
                    if (navigator.geolocation) {
                        // Call getCurrentPosition with success and failure callbacks
                        navigator.geolocation.getCurrentPosition(success, fail);
                    }
                    function success(position) {
                        //Found/approved to use current position
                        vm.propertyDetails.Lat = position.coords.latitude;
                        vm.propertyDetails.Long = position.coords.longitude;
                        setTimeout(function () { vm.initLocationData(position.coords.latitude, position.coords.longitude, true); }, 300);
                        angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call
                    }

                    function fail() {
                        // Could not obtain location, use U.S. defaults
                        vm.propertyDetails.Lat = 37.09024;
                        vm.propertyDetails.Long = -95.712891;
                        setTimeout(function () { vm.initLocationData(37.09024, -95.712891, true); }, 300);
                        angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call
                    }
                    
                }

                //vm.initLocationData(); //<-- Initialize map
            }

            if (vm.productProperties[index].ValueType == "datetime" )
            {
                if (vm.productProperties[index].AssignedValue == null)
                {
                    //Null datetimes need to be initiated with the current datetime
                    vm.propertyDetails.ValueToReplaceOrAppend = new Date();
                    vm.propertyDetails.PreviousValue = new Date();
                }

                //Initiate control in edit modal
                
                if (vm.productProperties[index].AssignedValue == null || vm.productProperties[index].AssignedValue == '') {
                    //if empty use current date/time
                    //Debug.trace("initiate new value")
                    //$('#edit-modal-datetime-picker').combodate({ value: new Date() });
                    $('#edit-modal-datetime-picker').combodate( 'setValue', new Date() );
                }
                else {
                    //Otherwise use the assigned value
                    //Debug.trace("use assigned value")
                    //$('#edit-modal-datetime-picker').combodate({ value: new Date(vm.productProperties[index].AssignedValue) });
                    $('#edit-modal-datetime-picker').combodate('setValue', new Date(vm.productProperties[index].AssignedValue));
                }
            }
        }

        vm.updateProductProperty = function (propertyValueType, closeModal) {

            var valueToReplace = vm.propertyDetails.ValueToReplaceOrAppend;

            vm.propertyDetails.ShowOptions = false;
            vm.propertyDetails.IsSending = true;
            vm.propertyDetails.SendingComplete = false;

            //Debug.trace("Updating product properties...");

            productServices.updateProductProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].ValueType, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, valueToReplace, 'replace')
            .success(function (data, status, headers, config) {
                
                vm.propertyDetails.SendingComplete = true;
                if(data.isSuccess)
                {
                    vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValue = vm.propertyDetails.ValueToReplaceOrAppend;
                    vm.propertyDetails.AssignedValue = vm.propertyDetails.ValueToReplaceOrAppend;

                    if (propertyValueType == 'predefined' || propertyValueType == 'swatch') {

                        if (vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues == null)
                        {
                            //If null make it an array to allow for pushing of first assigned value
                            vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = [];
                            vm.propertyDetails.AssignedValues = [];
                        }


                        vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues[0] = valueToReplace;
                        vm.propertyDetails.AssignedValues[0] = valueToReplace;
                        
                        
                    }
                    
                    if(closeModal)
                    {
                        $('.modal.in').modal('hide'); //<-- Hides the active modal
                        setTimeout(function () {
                            vm.propertyDetails.Clear();
                            vm.propertyDetails.IsSending = false;
                            console.log("cleared");
                        }, 270);
                    }
                    else
                    {
                        vm.propertyDetails.IsSending = false;
                    }
                    
                    //vm.globalPropertyEditingState = false;
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }

        vm.updateProductLocationProperty = function (closeModal) {

            vm.propertyDetails.ShowOptions = false;
            vm.propertyDetails.IsSending = true;
            vm.propertyDetails.SendingComplete = false;

            //Debug.trace("Updating product location property...");

            productServices.updateProductLocationProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].ValueType, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, vm.propertyDetails.LocationName, vm.propertyDetails.Address1, vm.propertyDetails.Address2, vm.propertyDetails.City, vm.propertyDetails.State, vm.propertyDetails.PostalCode, vm.propertyDetails.Country, vm.propertyDetails.Lat, vm.propertyDetails.Long)
            .success(function (data, status, headers, config) {
                
                vm.propertyDetails.SendingComplete = true;
                if (data.isSuccess) {

                    //Update Lat/Long & Show Map. Offer the user the Close or Edit Button (Forms get locked)
                    //Once locked (or if it already exists) we also display the Lat/Long
                    //SHOULD ALSO BE AN OPTION TO JUST CHOOSE A POINT ON THE MAP WITHOUT AN ADDRESS (LAT/LONG ONLY!!!!!!)

                    

                    //Update Properties Panel:

                    var locationUpdate = 
                        {
                            MapUrl : "https://www.google.com/maps/embed/v1/place?" + "q=" + vm.propertyDetails.Lat + ", " + vm.propertyDetails.Long + "&zoom=16&key=" + vm.googleMapsApiKey,
                            Name : vm.propertyDetails.LocationName,
                            Address1 : vm.propertyDetails.Address1,
                            Address2 : vm.propertyDetails.Address2,
                            City : vm.propertyDetails.City,
                            State : vm.propertyDetails.State,
                            PostalCode: vm.propertyDetails.PostalCode,
                            Country: vm.propertyDetails.Country,
                            Lat : vm.propertyDetails.Lat,
                            Long : vm.propertyDetails.Long,
                    }

                    vm.productProperties[vm.propertyDetails.CurrentIndex].LocationData = locationUpdate;
                    //if (!$scope.$$phase) {
                        //$digest or $apply
                    //}
                    //angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call


                    //vm.getProductProperties(); //<-- Refreshes Map/Data

                    if (closeModal) {
                        $('.modal.in').modal('hide'); //<-- Hides the active modal
                        
                        setTimeout(function () {
                            vm.propertyDetails.Clear();
                            vm.propertyDetails.IsSending = false;
                        }, 370);
                        
                    }
                    else {
                        vm.propertyDetails.IsSending = false;
                    }

                    //vm.globalPropertyEditingState = false;
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }


        //-----------Manageing Collection Based Properties ----------

        vm.movingCollectionProperty = false;

        vm.removeProductPropertyPredefinedCollectionItem = function (index) {

            vm.movingCollectionProperty = true;

            productServices.removeProductPropertyCollectionItem(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, index)
            .success(function (data, status, headers, config) {
                
                vm.movingCollectionProperty = false;

                if (data.isSuccess) {

                    vm.propertyDetails.PredefinedValuesDiff.push(vm.propertyDetails.AssignedValues[index]);
                    //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues.splice(index, 1); //<-- (Not needed) Holds a reference to the array (below) so they are in sync.
                    vm.propertyDetails.AssignedValues.splice(index, 1);  //<-- Updates the array above as well since they are already in sync.
                    
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
                vm.movingCollectionProperty = false;
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }

        vm.appendProductPropertyPredefinedCollectionItem = function (index) {

            vm.movingCollectionProperty = true;
            var valueToAppend = vm.propertyDetails.PredefinedValuesDiff[index];

            productServices.updateProductProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].ValueType, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, valueToAppend, 'append')
            .success(function (data, status, headers, config) {
                vm.movingCollectionProperty = false;
                if (data.isSuccess) {

                    vm.propertyDetails.PredefinedValuesDiff.splice(index, 1);

                    if (vm.propertyDetails.AssignedValues == null)
                    {
                        vm.propertyDetails.AssignedValues = [];
                    }
                    vm.propertyDetails.AssignedValues.push(valueToAppend);

                    if(vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues == null)
                    {
                        //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = []; //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                        vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = vm.propertyDetails.AssignedValues; //<-- Will now hold a reference to the other array so they are in sync
                    }
                    //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues.push(valueToAppend); //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
               
                vm.movingCollectionProperty = false;
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }

        // -- swatches

        vm.removeProductPropertySwatchCollectionItem = function (index) {

            vm.movingCollectionProperty = true;

            productServices.removeProductPropertyCollectionItem(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, index)
            .success(function (data, status, headers, config) {

                vm.movingCollectionProperty = false;

                if (data.isSuccess) {

                    vm.propertyDetails.AvailableSwatchesDiff.push(vm.propertyDetails.AssignedSwatches[index]);
                    //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues.splice(index, 1); //<-- (Not needed) Holds a reference to the array (below) so they are in sync.
                    vm.propertyDetails.AssignedSwatches.splice(index, 1);  //<-- Updates the array above as well since they are already in sync.

                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
                vm.movingCollectionProperty = false;
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }

        vm.appendProductPropertySwatchCollectionItem = function (index) {

            vm.movingCollectionProperty = true;
            var valueToAppend = vm.propertyDetails.AvailableSwatchesDiff[index].Label;

            productServices.updateProductProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].ValueType, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, valueToAppend, 'append')
            .success(function (data, status, headers, config) {
                vm.movingCollectionProperty = false;
                if (data.isSuccess) {

                    
                    if (vm.propertyDetails.AssignedSwatches == null) {
                        vm.propertyDetails.AssignedSwatches = [];
                    }
                    vm.propertyDetails.AssignedSwatches.push(vm.propertyDetails.AvailableSwatchesDiff[index]);

                    vm.propertyDetails.AvailableSwatchesDiff.splice(index, 1);


                    if (vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedSwatches == null) {
                        //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = []; //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                        vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedSwatches = vm.propertyDetails.AssignedSwatches; //<-- Will now hold a reference to the other array so they are in sync
                    }
                    //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues.push(valueToAppend); //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {

                vm.movingCollectionProperty = false;
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }

        vm.replaceProductPropertySwatchCollectionItem = function (index) {

            vm.movingCollectionProperty = true;
            var valueToReplace = vm.propertyDetails.AvailableSwatchesDiff[index].Label;

            productServices.updateProductProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].ValueType, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey, valueToReplace, 'replace')
            .success(function (data, status, headers, config) {
                vm.movingCollectionProperty = false;
                if (data.isSuccess) {

                    var swatchBeingReplaced = null;

                    if (vm.propertyDetails.AssignedSwatches == null) {
                        vm.propertyDetails.AssignedSwatches = [];
                        vm.propertyDetails.AssignedSwatches.push(vm.propertyDetails.AvailableSwatchesDiff[index]);
                    }
                    else {
                        swatchBeingReplaced = vm.propertyDetails.AssignedSwatches[0];
                        vm.propertyDetails.AssignedSwatches[0] = vm.propertyDetails.AvailableSwatchesDiff[index];
                    }
                    
                    vm.propertyDetails.AvailableSwatchesDiff.splice(index, 1);

                    if (swatchBeingReplaced != null)
                    {
                        vm.propertyDetails.AvailableSwatchesDiff.push(swatchBeingReplaced);
                    }

                    if (vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedSwatches == null) {
                        //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = []; //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                        vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedSwatches = vm.propertyDetails.AssignedSwatches; //<-- Will now hold a reference to the other array so they are in sync
                    }
                    //vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues.push(valueToAppend); //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                }
                else {
                    vm.propertyDetails.IsSending = false;
                    vm.propertyDetails.SendingComplete = true;
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {

                vm.movingCollectionProperty = false;
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }


        //--clear

        vm.clearProductProperty = function (closeModal)
        {
            vm.propertyDetails.ShowOptions = false;
            vm.propertyDetails.IsSending = true;
            vm.propertyDetails.SendingComplete = false;

            productServices.clearProductProperty(vm.product.FullyQualifiedName, vm.product.Name, vm.productProperties[vm.propertyDetails.CurrentIndex].NameKey)
            .success(function (data, status, headers, config) {
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                if (data.isSuccess) {

                    vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValue = null;
                    vm.productProperties[vm.propertyDetails.CurrentIndex].AssignedValues = null;

                    if (closeModal) {
                        $('.modal.in').modal('hide'); //<-- Hides the active modal
                        vm.propertyDetails.Clear();
                    }
                    else {
                        vm.propertyDetails.AssignedValue = null;
                        vm.propertyDetails.AssignedValues = [];
                    }

                }
                else {
                    vm.propertyDetails.Results.IsSuccess = false;
                    vm.propertyDetails.Results.Message = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {
                vm.propertyDetails.IsSending = false;
                vm.propertyDetails.SendingComplete = true;
                vm.propertyDetails.Results.IsSuccess = false;
                vm.propertyDetails.Results.Message = "Could not send update to server.";
            })
        }
        
        //--retry

        vm.retryPropertyEdit = function()
        {
            vm.propertyDetails.SendingComplete = false;
        }

        /*===========================================                  
               Datetime Helper Method                  
        ===========================================*/

        vm.toJsDate = function (str) {
            if (!str) return null;
            return new Date(str);
        }


        /*===========================================
         
                Location/Maps Modals
          
         ============================================*/

        vm.locationSubMenu =
            {
                addressButton: true,
                latLongButton: false,
                mapPointButton: false,

                update: function (buttonName) {

                    //Debug.trace(buttonName + " clicked");

                    if (buttonName == 'address') {
                        this.addressButton = true;
                        this.latLongButton = false;
                        this.mapPointButton = false;

                        //vm.getProperties();
                    }
                    else if (buttonName == 'latlong') {
                        this.addressButton = false;
                        this.latLongButton = true;
                        this.mapPointButton = false;

                        //vm.getTags();
                    }
                    else if (buttonName == 'mappoint') {
                        this.addressButton = false;
                        this.latLongButton = false;
                        this.mapPointButton = true;

                        //vm.getTags();
                    }
                },
            }

        /*===========================================    

                GOOGLE MAPS      

         ===========================================*/

        //Used to initiate maps (if account has location based properties)
        vm.initGoogleMaps = function () {
            // Enable the Google Maps JAVASCRIPT API in your Google API account!
            // Enable the Google STATIC Maps API in your Google API account!
            // Enable the Google Maps EMBED API in your Google API account!
            // Enable the Google Maps Geocoding API in your Google API account!

            $.getScript("https://maps.google.com/maps/api/js?v=3.23&key=" + vm.googleMapsApiKey);
        }

        vm.firstRun = true;

        vm.initLocationData = function(latIn, longIn, centerOnly)
        {

            //Debug.trace(latIn + ":" + longIn);
            // Adds a marker to the map.
            //vm.addMarker = function (location, map) {
                // Add the marker at the clicked location, and add the next-available label
                // from the array of alphabetical characters.
                //var marker = new google.maps.Marker({
                    //position: location,
                    //label: labels[labelIndex++ % labels.length],
                    //map: map
                //});
            //}

            if(vm.firstRun)
            {
                vm.setMarker = function (location, lat, lng, map) {
                    // Add the marker at the clicked location, and add the next-available label
                    // from the array of alphabetical characters.

                    //clear map of previous marker
                    if (currentMarker != null) {
                        currentMarker.setMap(null);

                    }

                    var marker = new google.maps.Marker({
                        position: location,
                        //label: labels[labelIndex++ % labels.length],
                        map: map
                    });

                    currentMarker = marker;

                    vm.propertyDetails.Lat = lat;
                    vm.propertyDetails.Long = lng;

                    vm.searchLatLongOk = true;

                    //Debug.trace("new lat:" + location.lat());
                    //Debug.trace("new long:" + location.lng());

                    angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call

                }

                vm.firstRun = false;

            }

            //Debug.trace("initializing map data with key: " + vm.googleMapsApiKey);

            // In the following example, markers appear when the user clicks on the map.
            // Each marker is labeled with a single alphabetical character.
            //var labels = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
            //var labelIndex = 0;

            var locationLatLong = { lat: +latIn, lng: +longIn };
            var currentMarker = null;
            //Debug.trace(locationLatLong);
            var map = new google.maps.Map(document.getElementById('google-map'), {
                zoom: 17,
                center: locationLatLong,
                streetViewControl: false, //<-- Hide street view
            });

            // This event listener calls addMarker() when the map is clicked.
            google.maps.event.addListener(map, 'click', function (event) {
                vm.setMarker(event.latLng, event.latLng.lat(), event.latLng.lng(), map);
            });

            
            if(centerOnly)
            {
                // No marker (first time adding details) remove the Lat/Long from the details and refresh the view
                vm.propertyDetails.Lat = null;
                vm.propertyDetails.Long = null;
                angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call
         
            }
            else {
                // Add a marker at the center of the map.
                vm.setMarker(locationLatLong, locationLatLong.lat, locationLatLong.lng, map);
            }
            
        }


        vm.centerMapOnAddress = function(type)
        {
            var geocoder, map;

            var addressSearch = ""; //vm.propertyDetails.Address1 + " " + vm.propertyDetails.Address2 + " " + vm.propertyDetails.City + " " + vm.propertyDetails.State + " " + vm.propertyDetails.PostalCode + " " + vm.propertyDetails.Country;

            if(type == 'name')
            {
                if (vm.propertyDetails.LocationName != null && vm.propertyDetails.LocationName != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.LocationName;
                }
            }
            else if (type == 'address') {
                if (vm.propertyDetails.Address1 != null && vm.propertyDetails.Address1 != '')
                {
                    addressSearch = addressSearch + vm.propertyDetails.Address1;
                }
                if (vm.propertyDetails.Address2 != null && vm.propertyDetails.Address2 != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.Address2;
                }
                if (vm.propertyDetails.City != null && vm.propertyDetails.City != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.City;
                }
                if (vm.propertyDetails.State != null && vm.propertyDetails.State != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.State;
                }
                if (vm.propertyDetails.PostalCode != null && vm.propertyDetails.PostalCode != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.PostalCode;
                }
                if (vm.propertyDetails.Country != null && vm.propertyDetails.Country != '') {
                    addressSearch = addressSearch + " " + vm.propertyDetails.Country;
                }
            }



            //Debug.trace(addressSearch);

            geocoder = new google.maps.Geocoder();
            geocoder.geocode({
                'address': addressSearch
            }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    var myOptions = {
                        zoom: 17,
                        center: results[0].geometry.location,
                       // mapTypeId: google.maps.MapTypeId.ROADMAP
                    }
                    
                    vm.initLocationData(results[0].geometry.location.lat(), results[0].geometry.location.lng(), false);
                    //Debug.trace(results[0].geometry.location + results[0].geometry.location.lat() + results[0].geometry.location.lng())
                    //map = new google.maps.Map(document.getElementById("google-map"), myOptions);
                    //vm.setMarker(results[0].geometry.location, results[0].geometry.location.lat(), results[0].geometry.location.lng(), map);
                    //var marker = new google.maps.Marker({
                        //map: map,
                       // position: results[0].geometry.location
                    //});
                }
            });
        }


        // Lat Long Advanced Editing ==============

        vm.editLatLong = false;
        vm.previousLat = null;
        vm.previousLong = null;
        vm.searchLatLongSuccess = false;
        vm.searchLatLongOk = true; //<-- Only used to show/hide save location button on location details modal

        vm.editLocationLatLong = function()
        {
            vm.searchLatLongSuccess = false;

            vm.editLatLong = true;
            vm.previousLat = vm.propertyDetails.Lat;
            vm.previousLong = vm.propertyDetails.Long;
        }

        vm.cancelEditLocationLatLong = function () {

            vm.searchLatLongSuccess = false;

            vm.editLatLong = false;

            if (vm.propertyDetails.Lat != vm.previousLat || vm.propertyDetails.Long != vm.previousLong)
            {
                vm.propertyDetails.Lat = vm.previousLat;
                vm.propertyDetails.Long = vm.previousLong;              
                setTimeout(function () {
                    vm.initLocationData(vm.propertyDetails.Lat, vm.propertyDetails.Long, false);
                }, 100);
                vm.searchLatLongOk = true;

            }
            
        }

        vm.searchLocationLatLong = function () {

            
            setTimeout(function () {
                vm.initLocationData(vm.propertyDetails.Lat, vm.propertyDetails.Long, false);
            }, 100);

            setTimeout(function () {

                //Debug.trace("Lat=" + isNaN(vm.propertyDetails.Lat));

                if (vm.propertyDetails.Lat != null && vm.propertyDetails.Lat != '' && !isNaN(vm.propertyDetails.Lat) && vm.propertyDetails.Long != null && !isNaN(vm.propertyDetails.Long) && vm.propertyDetails.Long != '') {
                    vm.searchLatLongSuccess = true;
                    vm.searchLatLongOk = true;
                }
                else {
                    vm.searchLatLongSuccess = false;
                    vm.searchLatLongOk = false;
                }

                angular.element(document.querySelector("#google-map")).scope().$apply(); //<--Update ANgular UI via scope().$apply() call

            }, 340);

        }

        vm.setLocationLatLong = function () {
            vm.editLatLong = false;
        }

        /*===========================================
          
               END PROPERTY METHODS
          
         ===========================================*/






        /*===========================================
          
                TAG METHODS
          
         ===========================================*/

        vm.tags = [];


        vm.getTags = function () {

            if (vm.tags.length > 0) {
                vm.tags.splice(0, vm.tags.length)
            }

            //Debug.trace("Getting tags...");

            tagServices.getTags()
            .success(function (data, status, headers, config) {

                var allTags = data;

                //var checked = [];

                if (allTags != null && allTags.length > 0) {
                    for (var t = 0, tl = allTags.length; t < tl; t++) {

                        var tag = {
                            Name: "",
                            Checked: false
                        };

                        tag.Name = allTags[t];
                        
                        if (vm.product.Tags != null && vm.product.Tags.length > 0) {
                            for (var p = 0, pl = vm.product.Tags.length; p < pl; p++) {
                                //alert("checking '" + allTags[t] + "'")
                                if (vm.product.Tags[p] == allTags[t]) {
                                    tag.Checked = true;
                                }
                                else if (vm.product.Tags[p] != allTags[t]) {
                                    //tag.Checked = false;
                                }
                            };
                        }
                        else if (vm.product.Tags == null) {
                            //tag.Checked = false;
                        }
                        //alert("evaluates:" + tag.Checked)
                        vm.tags.push(tag);
                        //checked.push(tag.Name);
                    };
 
                };

            })
            .error(function (data, status, headers, config) {

            })
        }
        vm.tagUpdating = false;


        vm.addProductTag = function (index) {

            vm.tagUpdating = true;

            //Debug.trace("adding tag...");
            var tagName = vm.tags[index].Name;
            vm.tags[index].Name = null

            productServices.addProductTag(vm.product.FullyQualifiedName, vm.product.Name, tagName)
            .success(function (data, status, headers, config) {

                vm.tagUpdating = false;

                if (data.isSuccess == true) {

                    vm.tags[index].Name = tagName;
                    vm.tags[index].Checked = true;
                    getProduct();
                }
                else {
                    vm.tags[index].Name = tagName;
                    vm.tags[index].Checked = false;
                }

            })
            .error(function (data, status, headers, config) {

                vm.tagUpdating = false;

                vm.tags[index].Name = tagName;
                vm.tags[index].Checked = false;
            })
        }

        vm.removeProductTag = function (index) {

            vm.tagUpdating = true;

            //Debug.trace("removing tag...");
            var tagName = vm.tags[index].Name;
            vm.tags[index].Name = null

            productServices.removeProductTag(vm.product.FullyQualifiedName, vm.product.Name, tagName)
            .success(function (data, status, headers, config) {

                vm.tagUpdating = false;

                if (data.isSuccess == true)
                {
                    vm.tags[index].Name = tagName;
                    vm.tags[index].Checked = false;
                    getProduct();
                }
                else {
                    vm.tags[index].Name = tagName;
                    vm.tags[index].Checked = true;
                }

            })
            .error(function (data, status, headers, config) {

                vm.tagUpdating = false;

                vm.tags[index].Name = tagName;
                vm.tags[index].Checked = true;
            })
        }










        /*=====================================================================
        ========================================================================
        ========================================================================

        START IMAGING METHODS
       
        
        ========================================================================*/


        /* ==========================================

          LOAD IMAGES

       ==========================================*/

        vm.imageObjectType = "product"; //<-- Must update on different TYPES
        vm.objectId = null; //<-- Load once and reuse
        vm.imagesReloaded = false;

        vm.imageRecords = null;
        vm.mainThumbnailUrl = null;

        vm.getImageRecords = function () {

            vm.objectId = vm.product.Id;

            imageServices.getImageRecordsForObject(vm.imageObjectType, vm.objectId, false)
            .success(function (data, status, headers, config) {

                vm.imageRecords = data;

                //If gallery ordering was just updated we reset details in modal window
                //if (vm.galleryUpdated)
                //{
                    //vm.galleryUpdated = false;
                    //vm.galleryUpdated = false;
                    //vm.loadImageDetails(vm.imageGroupNameKeyDetails, vm.imageGroupNameDetails, )
                //}

                //Turn on carousels for image gallery record types
                setTimeout(function () {

                    vm.imageRecords.forEach(function (group) {
                        group.ImageRecords.forEach(function (record) {
                            if (record.Type == 'gallery') {
                                var imageGalleryDivId = "carousel-" + group.GroupNameKey + "-" + record.FormatNameKey;

                                $("#" + imageGalleryDivId).carousel();

                            }

                            //Pull out main thumbnail
                            if (group.GroupNameKey == "default" && record.FormatNameKey == "thumbnail") {
                                vm.mainThumbnailUrl = record.Url;
                                angular.element(document.querySelector("#mainThumbnail")).scope().$apply();
                                //scope.$apply(); //<-- force UI refresh
                            }
                        });
                    });

                }, 110);

            })
            .error(function (data, status, headers, config) {

            })

        }

        /* ==========================================

          IMAGE DETAILS

       ==========================================*/

        vm.imageGroupNameKeyDetails = null;
        vm.imageGroupNameDetails = null;
        vm.imageRecordDetails = null;
        vm.resetImageDetailsTab = true;

        vm.loadImageDetails = function(imageGroupNameKey, imageGroupName, imageRecord)
        {
            vm.imageRecordDeleteApproval = false;
            vm.imageRecordDeleting = false;
            vm.imageGalleryIndexToDelete = null;

            vm.resetImageDetailsTab = true;

            //Debug.trace(imageRecord);

            vm.imageGroupNameKeyDetails = imageGroupNameKey;
            vm.imageGroupNameDetails = imageGroupName;
            vm.imageRecordDetails = imageRecord;

            // -- Reset image editing vars:
            vm.resetAllImageEditingVars();
        }


        /* ==========================================

           UPLOAD SOURCE IMAGE / Interact with Image Editng Modal to Upload Image Records

        ==========================================*/

        //vm.intermediateUrl = "init"; //<--After successful upload this is updated with the intermediary URL of the image to edit before creating a record

        //vm.uploadingIntermediaryImage = false;

        vm.firstImageUpload = true;

        vm.intermediaryWidth = null;
        vm.intermediaryHeight = null;

        //*Helper Methods -----------------------------------------------------------------------------------

        //Add function to image record/format slugs to trigger hidden file input when clicked
        vm.initiateIntermediaryImageUpload = function(imageFormat, imageGroupKey) {

            //vm.uploadingIntermediaryImage = true;

            vm.intermediaryTypeKey = vm.imageObjectType; 
            vm.intermediaryGroupKey = imageGroupKey;

            //Debug.trace(imageGroupKey);
            vm.intermediaryWidth = imageFormat.Width;
            vm.intermediaryHeight = imageFormat.Height;        
            vm.intermediaryFormatKey = imageFormat.FormatNameKey;
            vm.intermediaryFormatName = imageFormat.FormatName;


            //Debug.trace("initiating upload for " + imageFormat.FormatName);

            //Click's upload button on hidden file input
            $("#imageUploader:hidden").trigger('click');

        }

        // Add events to input to fire when status changes (MUST BE IN PARENT 'PARTIAL' - NOT CHILD 'TEMPLATE')
        document.getElementById("imageUploader").addEventListener('change', uploadIntermediaryImage);

        // Grab the files and set them to our variable and open the editing modal window
        function uploadIntermediaryImage() {

            //SHow intermediary loader:
            $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).fadeIn(200);
            //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).hide();
            
            /////////////////////////////////////////////

            //Fade out/clear all hover & error messages
            //$("#profilePhotoOverlay").fadeOut(150);
            //$("#photoUploadErrorText").text("");
            //$("#photoUploadErrorMessage").slideUp();

            /////////////////////////////////////////////

            // Check for the various HTML5 File API support.
            //if (window.File && window.FileReader && window.FileList && window.Blob) {

            if (window.File && window.FileReader && window.FileList && window.Blob) {

                // Great! All the File APIs are supported.
               

                //Debug.trace("Uploading Image...");// + vm.uploadingPhoto);

                var file = document.getElementById('imageUploader').files[0];

                //Debug.trace("photo ready for upload");
                //Debug.trace(file);
                //Debug.trace("source=" + file.src);

                //Debug.trace("size=" + file.size);
                //Debug.trace("type=" + file.type);
                //Debug.trace("result=" + file.result);

                if (file.size > 5000000)
                {
                    //Hide intermediary loader:
                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();

                    //showNotification
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text("Images must be smaller than 5mb");
                    setTimeout(function () {
                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                    }, 2000);

                    document.getElementById('imageUploader').value = '';

                }
                else if (file.type != "image/jpeg" && file.type != "image/png" && file.type != "image/gif" && file.type != "image/bmp") { // <-- Tiff features in future && file.type != "image/tiff"
                
                    //Debug.trace("Not a supported image format");
                    //vm.uploadingIntermediaryImage = false;

                    //Hide intermediary loader:
                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();
                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                    //showNotification("Not a supported file type!", "Alert", null, true);

                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text(file.type + " is not a supported file type!");
                    setTimeout(function () {
                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                    }, 2000);
                    

                    document.getElementById('imageUploader').value = '';

                    /////////////////////////////////////////////

                    //vm.uploadingPhoto = false;
                    //$("#uploadingPhotoAnimation").fadeOut(100);
                    //$("#photoUploadErrorText").text("Please only upload an image of type Jpeg, Png, Gif or Bmp.");
                    //$("#photoUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');

                    /////////////////////////////////////////////

                }
                //else if (file.size > "5000000") // (in Bytes) (5mb) 
                //{
                   // //Debug.trace("File too large");
                //}
                else {

                    var type = "jpg"; //<-- JPG & BMP will always convert to jpg

                    if(file.type == "image/png")
                    {
                        type = "png";
                        document.getElementById("imageEditingFrame").style.height = "570px";
                    }
                    else if (file.type == "image/gif")
                    {
                        type = "gif";
                        document.getElementById("imageEditingFrame").style.height = "570px";
                    }
                    else
                    {
                        document.getElementById("imageEditingFrame").style.height = "490px";
                    }



                    //Show Loader

                    /////////////////////////////////////////////

                    //$("#uploadingPhotoAnimation").fadeIn(200);

                    /////////////////////////////////////////////               

                    imageServices.uploadIntermediaryImageForObjectRecord(file, type, vm.intermediaryWidth, vm.intermediaryHeight).onreadystatechange = function () {
                        if (this.readyState == 4) { //<--0 = notinitialized, 1 = set up, 2 = sent, 3 = in process, 4 = complete

                            //Hide intermediary loader:
                            $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).slideUp(1000);
                            //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();


                            if (this.response != null && this.response != '') {
                            
                                //Debug.trace(this);
                                //Debug.trace("response1:" + this.response);
                                var data = JSON.parse(this.response);

                                if (data.isSuccess) {
 
                                    var editingUrl =
                                        '/Imaging/Instructions/' + '?SourceContainerName=' + data.SourceContainerName +
                                        '&ImageID=' + data.ImageId +
                                        '&FileName=' + data.FileName + '&FormatWidth=' + vm.intermediaryWidth + '&FormatHeight=' + vm.intermediaryHeight +
                                        '&SourceWidth=' + data.SourceWidth + '&SourceHeight=' + data.SourceHeight +
                                        '&ObjectType=' + vm.imageObjectType +
                                        '&ObjectID=' + vm.objectId +
                                        '&ImageGroupNameKey=' + vm.intermediaryGroupKey +
                                        '&ImageFormatNameKey=' + vm.intermediaryFormatKey +
                                        '&Type=' + type;

                                    //window.location.href = '/Imaging/Instructions/' + '?SourceContainerName=' + data.SourceContainerName + '&FileName=' + data.FileName + '&FormatWidth=' + vm.intermediaryWidth + '&FormatHeight=' + vm.intermediaryHeight;

                                        //Debug.trace(editingUrl);

                                    document.getElementById('imageEditingFrame').src = editingUrl;

                                    $('#imageEditModal').modal('show');
                                    $('#imageEditModal').modal({ show: true });

                                    //Add event to refresh images view on modal close (only once)
                                    if(vm.firstImageUpload)
                                    {
                                        vm.imagesReloaded = true; //<-- start from last image in gallery carousel view going forward

                                        $('#imageEditModal').on('hidden.bs.modal', function () {
                                            //alert("test");
                                            vm.getImageRecords();
                                            //vm.uploadingIntermediaryImage = false;
                                        })

                                        //vm.firstImageUpload = false;
                                    }
                                

                                    /**/
                                    //Clear input box in case same image is uploaded into a new slug:
                                    document.getElementById('imageUploader').value = ''; 

                                }
                                else {

                                
                                }
                            }
                        }
                        else {
                            
                            if (this.response != null && this.response != '')
                            {
                                //Debug.trace("response2:" + this.response);

                                var data = JSON.parse(this.response);

                                if (data.isSuccess == false) {
                                    //Clear input box in case same image is uploaded into a new slug:
                                    document.getElementById('imageUploader').value = '';

                                    //console.log("status:" + this.status);
                                    //console.log("readystate:" + this.readyState);
                                    //Hide intermediary loader:
                                    //$('#intermediaryImageUploading-' + vm.intermediaryGroupKey).fadeOut();
                                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                                    //Hide intermediary loader:
                                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();
                                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                                    //showNotification("Not a supported file type!", "Alert", null, true);

                                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text(data.ErrorMessage);
                                    setTimeout(function () {
                                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                                    }, 5500);
                                }
                            }
                            
                        }
                    }
                
                }

            }
            else
            {
                console.log("File API not supported in this browser");
            }
        }


        /* ==========================================

          Edit Titles/Descriptions

        ==========================================*/

        // - Single Image Vars

        vm.imageTitleEditing = false;
        vm.newImageRecordTitle = null;
        vm.imageTitleProcessing = false;

        vm.imageDescriptionEditing = false;
        vm.newImageRecordDescription = null;
        vm.imageDescriptionProcessing = false;

        // - Gallery Image Vars

        vm.imageGalleryTitleEditing = false;
        vm.newImageRecordGalleryTitle = null;
        vm.imageGalleryTitleProcessing = false;

        vm.imageGalleryDescriptionEditing = false;
        vm.newImageRecordGalleryDescription = null;
        vm.imageGalleryDescriptionProcessing = false;

        vm.visibleGalleryTitleEditingIndex = null;
        vm.visibleGalleryDescriptionEditingIndex = null;

        // - Global State   

        vm.globalImageEditingState = false;

        // -- Reset All Vars -------

        vm.resetAllImageEditingVars = function()
        {
            vm.imageTitleEditing = false;
            vm.newImageRecordTitle = null;
            vm.imageTitleProcessing = false;

            vm.imageDescriptionEditing = false;
            vm.newImageRecordDescription = null;
            vm.imageDescriptionProcessing = false;

            vm.imageGalleryTitleEditing = false;
            vm.newImageRecordGalleryTitle = null;
            vm.imageGalleryTitleProcessing = false;

            vm.imageGalleryDescriptionEditing = false;
            vm.newImageRecordGalleryDescription = null;
            vm.imageGalleryDescriptionProcessing = false;

            vm.visibleGalleryTitleEditingIndex = null;
            vm.visibleGalleryDescriptionEditingIndex = null;

            vm.globalImageEditingState = false;

            vm.visibleGalleryUrlIndex = null;
        }

        //-- Single Image Title --------------------------

        vm.editImageTitle = function()
        {
            vm.newImageRecordTitle = vm.imageRecordDetails.Title;
            vm.imageTitleEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageTitle = function () {

            //Debug.trace("Updating image title...");

            vm.imageTitleEditing = false;
            vm.imageTitleProcessing = true;

            imageServices.updateImageTitle(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, vm.newImageRecordTitle)
           .success(function (data, status, headers, config) {
           
               if (data.isSuccess) {

                   vm.imageRecordDetails.Title = vm.newImageRecordTitle;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }


        //-- Single Image Description --------------------------

        vm.editImageDescription = function () {
            vm.newImageRecordDescription = vm.imageRecordDetails.Description;
            vm.imageDescriptionEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageDescription = function () {

            //Debug.trace("Updating image description...");

            vm.imageDescriptionEditing = false;
            vm.imageDescriptionProcessing = true;

            imageServices.updateImageDescription(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, vm.newImageRecordDescription)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.Description = vm.newImageRecordDescription;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }

        //-- Gallery Image Title --------------------------

        vm.editImageGalleryTitle = function (index) {

            vm.visibleGalleryTitleEditingIndex = index;

            vm.newImageRecordGalleryTitle = vm.imageRecordDetails.GalleryImages[index].Title;
            vm.imageGalleryTitleEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageGalleryTitle = function (index) {

            //Debug.trace("Updating image gallery title...");

            vm.imageGalleryTitleEditing = false;
            vm.imageGalleryTitleProcessing = true;

            imageServices.updateGalleryTitle(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, index, vm.newImageRecordGalleryTitle)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.GalleryImages[index].Title = vm.newImageRecordGalleryTitle;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }


        //-- Gallery Image Description --------------------------

        vm.editImageGalleryDescription = function (index) {

            vm.visibleGalleryDescriptionEditingIndex = index;

            vm.newImageRecordGalleryDescription = vm.imageRecordDetails.GalleryImages[index].Description;
            vm.imageGalleryDescriptionEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageGalleryDescription = function (index) {

            //Debug.trace("Updating image gallery description...");

            vm.imageGalleryDescriptionEditing = false;
            vm.imageGalleryDescriptionProcessing = true;

            imageServices.updateGalleryDescription(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, index, vm.newImageRecordGalleryDescription)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.GalleryImages[index].Description = vm.newImageRecordGalleryDescription;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }



        /* ==========================================
               
            Reorder Gallery
               
        ==========================================*/

        vm.newGalleryImageOrder = null;
        vm.orgGalleryImageOrder = null;
        vm.imageOrderProcessing = false;

        //vm.galleryUpdated = false;

       vm.initializeImageGalleryOrderPanel = function () {

            //vm.newGalleryImageOrder = vm.imageRecordDetails.GalleryImages;

            vm.cancelImageOrdering();
            vm.newGalleryImageOrder = [];
            vm.orgGalleryImageOrder = [];

            //vm.newOrder = vm.category.slice(); //<-- Depricated

            //Pipe in values and rename to generic items----------------
            for (var i = 0, len = vm.imageRecordDetails.GalleryImages.length; i < len; ++i) {
                var galleryImage = { index: i, url: vm.imageRecordDetails.GalleryImages[i].Url_sm };
                vm.newGalleryImageOrder.push(galleryImage);
            };

            vm.orgGalleryImageOrder = vm.newGalleryImageOrder;
        }

        vm.cancelImageOrdering = function()
        {
            vm.newGalleryImageOrder = null;
            vm.orgGalleryImageOrder = null;
        }

        vm.updateImageGalleryOrdering = function () {

            vm.imageOrderProcessing = true;

            //Debug.trace("Updating image gallery ordering...");

            //Create array of indexes in the new order ----
            var indexList = vm.newGalleryImageOrder.map(function (item) {
                return item['index'];
            });

            vm.imageGalleryOrderProcessing = true;

            imageServices.reorderGallery(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, indexList)
           .success(function (data, status, headers, config) {

               vm.imageOrderProcessing = false;

               if (data.isSuccess) {

                   $('#imageOrderingSuccess').fadeIn();

                   setTimeout(function () {
                       $('#imageOrderingSuccess').fadeOut();
                   }, 1800);

                   //vm.imageRecordDetails.GalleryImages[index].Description = vm.newImageRecordGalleryDescription;
                   //vm.resetAllImageEditingVars();
                   //vm.galleryUpdated = true; //<-- will update ordering of gallery detail modal if true after updating records from CoreServices (below)
                   vm.getImageRecords();
                   
                   //Reorder items in details

                   //Pipe in values to reorder items in details----------------
                   var newGalleryImagesArray = [];

                   for (var i = 0, len = vm.newGalleryImageOrder.length; i < len; ++i) {
                       newGalleryImagesArray.push(vm.imageRecordDetails.GalleryImages[vm.newGalleryImageOrder[i].index]);
                   };

                   vm.imageRecordDetails.GalleryImages = newGalleryImagesArray;

               }
               else {
                   //vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {

               vm.imageOrderProcessing = false;
               //vm.resetAllImageEditingVars();
           })
        }

        /* ==========================================
       
          Show/Hide Gallery Image URLs
       
        ==========================================*/

        vm.visibleGalleryUrlIndex = null;

        vm.setVisibleGalleryUrlIndex = function(indexValue)
        {
            vm.visibleGalleryUrlIndex = indexValue;
        }

        /* ==========================================

          DELETE/CLEAR IMAGE RECORD

        ==========================================*/

        vm.imageRecordDeleteApproval = false;
        vm.imageRecordDeleting = false;

        vm.approveImageRecordDeletion = function()
        {
            vm.imageRecordDeleteApproval = true;
            vm.imageRecordDeleting = false;
        }

        vm.disproveImageRecordDeletion = function () {
            vm.imageRecordDeleteApproval = false;
            vm.imageRecordDeleting = false;
        }

        vm.deleteImageRecord = function () {

            vm.imageRecordDeleting = true;

            imageServices.deleteImageRecordForObject(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey)
            .success(function (data, status, headers, config) {

                vm.getImageRecords();
                $('.modal.in').modal('hide');

                //Refresh main thumnail:
                angular.element(document.querySelector("#mainThumbnail")).scope().$apply();
            })
            .error(function (data, status, headers, config) {
                vm.imageRecordDeleteApproval = false;
                vm.imageRecordDeleting = false;
            })

        }




        /* ==========================================

          DELETE/CLEAR IMAGE GALLERY ITEM

        ==========================================*/

        vm.imageGalleryIndexToDelete = null;

        vm.deleteImageGalleryItem = function (imageIndex) {

            vm.imageGalleryIndexToDelete = imageIndex;

            imageServices.deleteGalleryImageForObject(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, imageIndex)
            .success(function (data, status, headers, config) {

                if(data.isSuccess)
                {
                    vm.imageRecordDetails.GalleryImages.splice(imageIndex, 1);
                }

                vm.imageGalleryIndexToDelete = null;
                vm.getImageRecords();

                //If this was the last image in the array we close the modal:
                if (vm.imageRecordDetails.GalleryImages.length == 0)
                {
                    $('.modal.in').modal('hide');
                }


            })
            .error(function (data, status, headers, config) {
                vm.imageGalleryIndexToDelete = null;
            })

        }

        /*=====================================================================


        END IMAGING METHODS
       
        ========================================================================
        ========================================================================
        ========================================================================*/

















        /* ==========================================

           DELETE PRODUCT

       ==========================================*/

        vm.categorizationTypeKey; //<-- Used for link back (Shared with MOVE method below)

        vm.productDeletion =
            {
                Verify: false,
                Complete: false,
                Processing: false,
                IsSuccess: false,
                Message: null,

                reset: function () {
                    this.Complete = false,
                    this.IsSuccess = false,
                    this.Message = null,
                    this.Processing = false
                    this.Verify = false;
                }
            }

        vm.startDeletion = function () {
            vm.productDeletion.Verify = true;
        }

        vm.cancelDeletion = function () {
            vm.productDeletion.reset();
        }

        vm.deleteproduct = function (productId) {

            vm.productDeletion.Verify = false;
            vm.productDeletion.Processing = true;

            //Debug.trace("Deleting product...");

            productServices.deleteProduct(vm.product.Id)
            .success(function (data, status, headers, config) {

                vm.productDeletion.Processing = false;
                vm.productDeletion.Complete = true;

                if (data.isSuccess) {

                    vm.productDeletion.IsSuccess = true;
                    vm.productDeletion.Message = "This item has been deleted.";

                    //Shared with MOVE method below
                    var path = vm.product.LocationPath.split("/");

                    if (path.length == 1) {
                        vm.categorizationTypeKey = "category";
                    }
                    if (path.length == 2) {
                        vm.categorizationTypeKey = "subcategory";
                    }
                    if (path.length == 3) {
                        vm.categorizationTypeKey = "subsubcategory";
                    }
                    if (path.length == 4) {
                        vm.categorizationTypeKey = "subsubsubcategory";
                    }
                }
                else {

                    vm.productDeletion.IsSuccess = false;
                    vm.productDeletion.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.productDeletion.Processing = false;
                    vm.productDetailUpdates.IsSuccess = false;
                    vm.productDetailUpdates.Complete = true;
                    vm.productDetailUpdates.Message = "An error occurred while attempting to use the service...";

                })


        }

        /* ==========================================

          END DELETE PRODUCT

       ==========================================*/



        /*===========================================
          
                MOVE METHODS
          
         ===========================================*/

        vm.newProductUrl = null;

        vm.categories = null;
        vm.subcategories = null;
        vm.subsubcategories = null;
        vm.subsubsubcategories = null;

        vm.productMove =
            {
                //ProposedLocation: "(TBD)",

                Category: null,
                Subcategory: null,
                Subsubcategory: null,
                Subsubsubcategory: null,

                NewLocationPath: null,

                Ready: false,
                Complete: false,
                Processing: false,
                IsSuccess: false,
                Message: null,

                reset: function () {

                    //this.ProposedLocation = "(TBD)",

                    this.Category = null;
                    this.Subcategory = null;
                    this.Subsubcategory = null;
                    this.Subsubsubcategory = null;

                    this.Ready = false,
                    this.Complete = false,
                    this.Processing = false
                    this.IsSuccess = false;
                    this.Message = null;
                },

                updateCategory: function (category) {
                    this.Category = category;
                    //Update Location Path
                    vm.productMove.NewLocationPath = vm.productMove.Category.CategoryNameKey;
                    //vm.productMove.Ready = true;
                    vm.getSubcategories();
                },
                updateSubcategory: function (subcategory) {
                    this.Subcategory = subcategory;
                    //Update Location Path
                    vm.productMove.NewLocationPath = vm.productMove.Category.CategoryNameKey + "/" + vm.productMove.Subcategory.SubcategoryNameKey;
                    //Anything after subcategories is OK for moves
                    //vm.productMove.Ready = true;
                    vm.getSubsubcategories();
                },
                updateSubsubcategory: function (subsubcategory) {
                    this.Subsubcategory = subsubcategory;
                    //Update Location Path
                    vm.productMove.NewLocationPath = vm.productMove.Category.CategoryNameKey + "/" + vm.productMove.Subcategory.SubcategoryNameKey + "/" + vm.productMove.Subsubcategory.SubsubcategoryNameKey;
                    vm.getSubsubsubcategories();
                },
                updateSubsubsubcategory: function (subsubsubcategory) {
                    this.Subsubsubcategory = subsubsubcategory;
                    //Move ready ALWAYS true this many levels deep
                    vm.productMove.Ready = true;
                    //Update Location Path
                    vm.productMove.NewLocationPath = vm.productMove.Category.CategoryNameKey + "/" + vm.productMove.Subcategory.SubcategoryNameKey + "/" + vm.productMove.Subsubcategory.SubsubcategoryNameKey + "/" + vm.productMove.Subsubsubcategory.SubsubsubcategoryNameKey;
                },

            }


        vm.startProductMove = function()
        {
            vm.productMove.NewLocationPath = null;

            //Reset the productMove object:
            vm.productMove.reset();

            //Nullify self and all children
            vm.categories = null;
            vm.subcategories = null;
            vm.subsubcategories = null;
            vm.subsubsubcategories = null;

            //Get category list:
            categoryServices.getCategories()
            .success(function (data, status, headers, config) {
                vm.categories = data;
            })
            .error(function (data, status, headers, config) {
            })
        }

        vm.moveProduct = function () {

            vm.productMove.Processing = true;
            vm.productMove.Complete = false;

            productServices.moveProduct(vm.product.Id, vm.productMove.NewLocationPath)
            .success(function (data, status, headers, config) {
                vm.productMove.Processing = false;
                vm.productMove.Complete = true;
                if (data.isSuccess)
                {
                    vm.productMove.IsSuccess = true;
                    vm.productMove.Message = "Item has been moved!";

                    //Find path back to parent (Shared with Delete Method)
                    var path = vm.product.LocationPath.split("/");

                    if (path.length == 1) {
                        vm.categorizationTypeKey = "category";
                    }
                    if (path.length == 2) {
                        vm.categorizationTypeKey = "subcategory";
                    }
                    if (path.length == 3) {
                        vm.categorizationTypeKey = "subsubcategory";
                    }
                    if (path.length == 4) {
                        vm.categorizationTypeKey = "subsubsubcategory";
                    }

                    //Set path to new product location:
                    vm.newProductUrl = "item/" +vm.productMove.NewLocationPath + "/" +vm.product.NameKey;


                }
                else {
                    vm.productMove.IsSuccess = false;
                    vm.productMove.Message = data.ErrorMessage;
                    }
            })
            .error(function (data, status, headers, config) {
                vm.productMove.Processing = false;
                vm.productMove.Complete = true;
                vm.productMove.IsSuccess = false;
                vm.productMove.Message = "An unknown error occurred.";
            })
        }

        vm.getSubcategories = function()
        {
            //Anything before subcategories is NOT OK for moves
            //vm.productMove.Ready = false,

            //Nullify self and all children
            vm.subcategories = null;
            vm.subsubcategories = null;
            vm.subsubsubcategories = null;

            vm.productMove.Subcategory = null;
            vm.productMove.Subsubcategory = null;
            vm.productMove.Subsubsubcategory = null;

            //Get list:
            categoryServices.getCategory(vm.productMove.Category.CategoryNameKey)
            .success(function (data, status, headers, config) {
                vm.subcategories = data.Subcategories;

                if(vm.subcategories.length == 0)
                {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = true;
                }
                else {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = false;
                }

                /*
                var path = vm.product.LocationPath.split("/");
                if (path.length == 2) {
                    //If this product is in a subcategory, remove that subcategory from the list of available move to options:
                    for (var i = 0; i < vm.subcategories.length; i++)
                        if (vm.subcategories[i].SubcategoryNameKey === path[1]) {
                            vm.subcategories.splice(i, 1);
                            break;
                        }
                }*/


            })
            .error(function (data, status, headers, config) {
            })
        }


        vm.getSubsubcategories = function () {

            //Nullify self and all children
            vm.subsubcategories = null;
            vm.subsubsubcategories = null;

            vm.productMove.Subsubcategory = null;
            vm.productMove.Subsubsubcategory = null;

            //Get list:
            categoryServices.getSubcategory(vm.productMove.Category.CategoryNameKey, vm.productMove.Subcategory.SubcategoryNameKey)
            .success(function (data, status, headers, config) {
                vm.subsubcategories = data.Subsubcategories;

                if (vm.subsubcategories.length == 0) {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = true;
                }
                else {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = false;
                }

                /*
                var path = vm.product.LocationPath.split("/");
                if (path.length == 3) {
                    //If this product is in a subsubcategory, remove that subsubcategory from the list of available move to options:
                    for (var i = 0; i < vm.subsubcategories.length; i++)
                        if (vm.subsubcategories[i].SubsubcategoryNameKey === path[2]) {
                            vm.subsubcategories.splice(i, 1);
                            break;
                        }
                }*/
            })
            .error(function (data, status, headers, config) {
            })
        }

        vm.getSubsubsubcategories = function () {

            //Nullify self and all children
            vm.subsubsubcategories = null;

            vm.productMove.Subsubsubcategory = null;

            //Get list:
            categoryServices.getSubsubcategory(vm.productMove.Category.CategoryNameKey, vm.productMove.Subcategory.SubcategoryNameKey, vm.productMove.Subsubcategory.SubsubcategoryNameKey)
            .success(function (data, status, headers, config) {
                vm.subsubsubcategories = data.Subsubsubcategories;

                if (vm.subsubsubcategories.length == 0) {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = true;
                }
                else {
                    //We only allow moves into categorizations that do not have subcategories listed
                    vm.productMove.Ready = false;
                }

                var path = vm.product.LocationPath.split("/");
                if (path.length == 4) {
                    //If this product is in a subsubsubcategory, remove that subsubcategory from the list of available move to options:
                    for (var i = 0; i < vm.subsubsubcategories.length; i++)
                        if (vm.subsubsubcategories[i].SubsubsubcategoryNameKey === path[3]) {
                            vm.subsubsubcategories.splice(i, 1);
                            break;
                        }
                }
            })
            .error(function (data, status, headers, config) {
            })
        }


        /*==========================================
              General Account Settings
         ==========================================*/

        vm.accountSettings = null;

        vm.getAccountSettings = function () {

            accountSettingsServices.getAccountSettings()
            .success(function (data, status, headers, config) {

                vm.accountSettings = data;

            })
            .error(function (data, status, headers, config) {

            })
        }


        /* ==========================================
               Submit Sales Lead For Product
        ==========================================*/

        vm.salesLead =
        {
            Label: "New",
            Origin: "Admin",

            FirstName: null,
            LastName: null,
            CompanyName: null,

            Phone: null,
            Email: null,
            Comments: null,
            Notes: null,

            IsSending: false,

            IsSuccess: null,
            ErrorMessage: null,

            Clear: function () {

                this.FirstName = null,
                this.LastName = null,
                this.CompanyName = null,
                this.Phone = null,
                this.Email = null,
                this.Comments = null,
                this.Notes = null,

                this.IsSending = false;

                this.IsSuccess = null;
                this.ErrorMessage = null;
            }
        }

        vm.submitSalesLead = function()
        {
            vm.salesLead.IsSending = true;

            leadsServices.submitSalesLead(vm.salesLead.Label, vm.salesLead.FirstName, vm.salesLead.LastName, vm.salesLead.CompanyName, vm.salesLead.Phone, vm.salesLead.Email, vm.salesLead.Comments, vm.salesLead.Notes, vm.product.Name, vm.product.Id, vm.product.FullyQualifiedName, vm.product.LocationPath, vm.salesLead.Origin, '', '', '')
                    .success(function (data, status, headers, config) {

                        vm.salesLead.IsSending = false;

                        if(data.isSuccess)
                        {
                            
                            vm.salesLead.IsSuccess = true;
                        }
                        else {
                            vm.salesLead.IsSuccess = false;
                            vm.salesLead.ErrorMessage = data.ErrorMessage;
                        }

                    })
                    .error(function (data, status, headers, config) {
                        
                        vm.salesLead.IsSuccess = false;
                        vm.salesLead.ErrorMessage = "Unknown error";

                    })
        }
       


        /* ==========================================
               Base Controller Methods
        ==========================================*/

        vm.product = null;

        function getProduct() {
            //Debug.trace("Getting product...");

            productServices.getProduct(vm.fullyQualifiedPath)
            .success(function (data, status, headers, config) {

                vm.product = data;
                vm.getProductProperties()
                vm.getImageRecords();

                //Debug.trace("Product received!");

                getCategorization();

            })
                .error(function (data, status, headers, config) {


                })
        }

        function getCategorization() {
            //Debug.trace("Getting categorization...");

            if(vm.subsubsubcategoryNameKey != null)
            {
                categoryServices.getSubsubsubcategory(vm.categoryNameKey, vm.subcategoryNameKey, vm.subsubcategoryNameKey, vm.subsubsubcategoryNameKey)
                .success(function (data, status, headers, config) {
                    vm.subsubsubcategory = data;
                    vm.subsubsubcategoryName = vm.subsubsubcategory.SubsubsubcategoryName
                    vm.subsubcategoryName = vm.subsubsubcategory.Subsubcategory.SubsubcategoryName
                    vm.subcategoryName = vm.subsubsubcategory.Subcategory.SubcategoryName
                    vm.categoryName = vm.subsubsubcategory.Category.CategoryName
                }).error(function (data, status, headers, config) { })
            }
            else if (vm.subsubcategoryNameKey != null) {
                categoryServices.getSubsubcategory(vm.categoryNameKey, vm.subcategoryNameKey, vm.subsubcategoryNameKey)
                .success(function (data, status, headers, config) {
                    vm.subsubcategory = data;
                    vm.subsubcategoryName = vm.subsubcategory.SubsubcategoryName
                    vm.subcategoryName = vm.subsubcategory.Subcategory.SubcategoryName
                    vm.categoryName = vm.subsubcategory.Category.CategoryName
                }).error(function (data, status, headers, config) { })
            }
            else if (vm.subcategoryNameKey != null) {
                categoryServices.getSubcategory(vm.categoryNameKey, vm.subcategoryNameKey)
                .success(function (data, status, headers, config) {
                    vm.subcategory = data;
                    vm.subcategoryName = vm.subcategory.SubcategoryName
                    vm.categoryName = vm.subcategory.Category.CategoryName
                }).error(function (data, status, headers, config) { })
            }
            else if (vm.categoryNameKey != null) {
                categoryServices.getCategory(vm.categoryNameKey)
                .success(function (data, status, headers, config) {
                    vm.category = data;
                    vm.categoryName = vm.category.CategoryName
                }).error(function (data, status, headers, config) { })
            }

        }

        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null)
                {
                    //Log user out if empty
                    window.location.replace("/login");
                }

                //Debug.trace("Profile refreshed!");
                //Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);
            vm.googleMapsApiKey = JSON.parse(CoreServiceSettings_GoogleMapsApiKey);


            //For <legal-footer></legal-footer>
            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();


            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "")
            {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);

            getProduct();
            vm.getAccountSettings();

            //Debug.trace('productIndexController activation complete');


            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!

                //Debug.trace("Lowest role allowed: '" + lowestRoleAllowed + "'");
                //Debug.trace("Comparing: User: '" + currentUserRoleIndex + "' Allowed: '" + allowedIndex + "'");

                if (currentUserRoleIndex >= allowedIndex) {
                    //Debug.trace("Allowed!");
                    return true;
                }
                else {
                    //Debug.trace("Not allowed!");
                    return false;
                }
            }

            //Bool: Checks if the users role is below allowed Role
            vm.reverseRole = function (lowestRoleAllowed) {
                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!
                if (currentUserRoleIndex < allowedIndex) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

    }

})();

