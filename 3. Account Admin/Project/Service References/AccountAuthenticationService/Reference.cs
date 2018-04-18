﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AccountAdminSite.AccountAuthenticationService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AuthenticationResponse", Namespace="http://schemas.datacontract.org/2004/07/WCF.WcfEndpoints.Contracts.Account")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(AccountAdminSite.AccountAuthenticationService.AccountUser))]
    public partial class AuthenticationResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private AccountAdminSite.AccountAuthenticationService.AccountUser AccountUserField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Security.Claims.ClaimsIdentity ClaimsIdentityField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorMessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool isSuccessField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public AccountAdminSite.AccountAuthenticationService.AccountUser AccountUser {
            get {
                return this.AccountUserField;
            }
            set {
                if ((object.ReferenceEquals(this.AccountUserField, value) != true)) {
                    this.AccountUserField = value;
                    this.RaisePropertyChanged("AccountUser");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Security.Claims.ClaimsIdentity ClaimsIdentity {
            get {
                return this.ClaimsIdentityField;
            }
            set {
                if ((object.ReferenceEquals(this.ClaimsIdentityField, value) != true)) {
                    this.ClaimsIdentityField = value;
                    this.RaisePropertyChanged("ClaimsIdentity");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorMessage {
            get {
                return this.ErrorMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorMessageField, value) != true)) {
                    this.ErrorMessageField = value;
                    this.RaisePropertyChanged("ErrorMessage");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool isSuccess {
            get {
                return this.isSuccessField;
            }
            set {
                if ((this.isSuccessField.Equals(value) != true)) {
                    this.isSuccessField = value;
                    this.RaisePropertyChanged("isSuccess");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AccountUser", Namespace="http://schemas.datacontract.org/2004/07/Sahara.Core.Accounts.Models")]
    [System.SerializableAttribute()]
    public partial class AccountUser : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid AccountIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AccountNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AccountNameKeyField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool AccountOwnerField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool ActiveField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> CreatedDateField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FirstNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FullNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PhotoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RoleField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid AccountID {
            get {
                return this.AccountIDField;
            }
            set {
                if ((this.AccountIDField.Equals(value) != true)) {
                    this.AccountIDField = value;
                    this.RaisePropertyChanged("AccountID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AccountName {
            get {
                return this.AccountNameField;
            }
            set {
                if ((object.ReferenceEquals(this.AccountNameField, value) != true)) {
                    this.AccountNameField = value;
                    this.RaisePropertyChanged("AccountName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AccountNameKey {
            get {
                return this.AccountNameKeyField;
            }
            set {
                if ((object.ReferenceEquals(this.AccountNameKeyField, value) != true)) {
                    this.AccountNameKeyField = value;
                    this.RaisePropertyChanged("AccountNameKey");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool AccountOwner {
            get {
                return this.AccountOwnerField;
            }
            set {
                if ((this.AccountOwnerField.Equals(value) != true)) {
                    this.AccountOwnerField = value;
                    this.RaisePropertyChanged("AccountOwner");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Active {
            get {
                return this.ActiveField;
            }
            set {
                if ((this.ActiveField.Equals(value) != true)) {
                    this.ActiveField = value;
                    this.RaisePropertyChanged("Active");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> CreatedDate {
            get {
                return this.CreatedDateField;
            }
            set {
                if ((this.CreatedDateField.Equals(value) != true)) {
                    this.CreatedDateField = value;
                    this.RaisePropertyChanged("CreatedDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FirstName {
            get {
                return this.FirstNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FirstNameField, value) != true)) {
                    this.FirstNameField = value;
                    this.RaisePropertyChanged("FirstName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FullName {
            get {
                return this.FullNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FullNameField, value) != true)) {
                    this.FullNameField = value;
                    this.RaisePropertyChanged("FullName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id {
            get {
                return this.IdField;
            }
            set {
                if ((object.ReferenceEquals(this.IdField, value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastName {
            get {
                return this.LastNameField;
            }
            set {
                if ((object.ReferenceEquals(this.LastNameField, value) != true)) {
                    this.LastNameField = value;
                    this.RaisePropertyChanged("LastName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Photo {
            get {
                return this.PhotoField;
            }
            set {
                if ((object.ReferenceEquals(this.PhotoField, value) != true)) {
                    this.PhotoField = value;
                    this.RaisePropertyChanged("Photo");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Role {
            get {
                return this.RoleField;
            }
            set {
                if ((object.ReferenceEquals(this.RoleField, value) != true)) {
                    this.RoleField = value;
                    this.RaisePropertyChanged("Role");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserName {
            get {
                return this.UserNameField;
            }
            set {
                if ((object.ReferenceEquals(this.UserNameField, value) != true)) {
                    this.UserNameField = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AccountAuthenticationService.IAccountAuthenticationService")]
    public interface IAccountAuthenticationService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/Authenticate", ReplyAction="http://tempuri.org/IAccountAuthenticationService/AuthenticateResponse")]
        AccountAdminSite.AccountAuthenticationService.AuthenticationResponse Authenticate(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/Authenticate", ReplyAction="http://tempuri.org/IAccountAuthenticationService/AuthenticateResponse")]
        System.Threading.Tasks.Task<AccountAdminSite.AccountAuthenticationService.AuthenticationResponse> AuthenticateAsync(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/GetClaimsIdentity", ReplyAction="http://tempuri.org/IAccountAuthenticationService/GetClaimsIdentityResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AccountAdminSite.AccountAuthenticationService.AuthenticationResponse))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AccountAdminSite.AccountAuthenticationService.AccountUser))]
        System.Security.Claims.ClaimsIdentity GetClaimsIdentity(string userName, string sharedClientKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/GetClaimsIdentity", ReplyAction="http://tempuri.org/IAccountAuthenticationService/GetClaimsIdentityResponse")]
        System.Threading.Tasks.Task<System.Security.Claims.ClaimsIdentity> GetClaimsIdentityAsync(string userName, string sharedClientKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/Logout", ReplyAction="http://tempuri.org/IAccountAuthenticationService/LogoutResponse")]
        void Logout(string userName, string sharedClientKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAccountAuthenticationService/Logout", ReplyAction="http://tempuri.org/IAccountAuthenticationService/LogoutResponse")]
        System.Threading.Tasks.Task LogoutAsync(string userName, string sharedClientKey);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAccountAuthenticationServiceChannel : AccountAdminSite.AccountAuthenticationService.IAccountAuthenticationService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AccountAuthenticationServiceClient : System.ServiceModel.ClientBase<AccountAdminSite.AccountAuthenticationService.IAccountAuthenticationService>, AccountAdminSite.AccountAuthenticationService.IAccountAuthenticationService {
        
        public AccountAuthenticationServiceClient() {
        }
        
        public AccountAuthenticationServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public AccountAuthenticationServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AccountAuthenticationServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AccountAuthenticationServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public AccountAdminSite.AccountAuthenticationService.AuthenticationResponse Authenticate(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey) {
            return base.Channel.Authenticate(accountName, email, password, ipAddress, origin, sharedClientKey);
        }
        
        public System.Threading.Tasks.Task<AccountAdminSite.AccountAuthenticationService.AuthenticationResponse> AuthenticateAsync(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey) {
            return base.Channel.AuthenticateAsync(accountName, email, password, ipAddress, origin, sharedClientKey);
        }
        
        public System.Security.Claims.ClaimsIdentity GetClaimsIdentity(string userName, string sharedClientKey) {
            return base.Channel.GetClaimsIdentity(userName, sharedClientKey);
        }
        
        public System.Threading.Tasks.Task<System.Security.Claims.ClaimsIdentity> GetClaimsIdentityAsync(string userName, string sharedClientKey) {
            return base.Channel.GetClaimsIdentityAsync(userName, sharedClientKey);
        }
        
        public void Logout(string userName, string sharedClientKey) {
            base.Channel.Logout(userName, sharedClientKey);
        }
        
        public System.Threading.Tasks.Task LogoutAsync(string userName, string sharedClientKey) {
            return base.Channel.LogoutAsync(userName, sharedClientKey);
        }
    }
}