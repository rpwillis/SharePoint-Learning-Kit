/* Copyright (c) Microsoft Corporation. All rights reserved. */

// RteApiSite.js
// This file contains the RteApiSite object and all functions required to implement it. Functions that 
// begin with Site_ are exposed on the object as public apis. Other functions should not be called from
// outside this file.

var g_framesetMgr;

// The RteApiSite object is receives requests from the RTE objects, stores the data in a form
// that is easily accessible and communicates with the frameset to send and receive data to/from
// the server. This object does not validate any RTE values.
function RteApiSite( framesetMgr )
{
    // Set global variables
    g_framesetMgr = framesetMgr;
    
    // Initialize variables that will hold the data model information
    this.m_aValues = new Array();  // holds complete data model
    this.m_aChangedValues = new Array();   // holds only elements that have changed in Values array.
	 
	// Functions called from RTE
	this.Terminate = Site_Terminate;
	this.Commit = Site_Commit;
	this.SetValue = Site_SetValue;
	this.GetValue = Site_GetValue;
	this.IsContinueRequestValid = Site_IsContinueRequestValid;
	this.IsPreviousRequestValid = Site_IsPreviousRequestValid;
	this.IsChoiceRequestValid = Site_IsChoiceRequestValid;
	this.ClearError = Site_ClearError;
	this.SetError = Site_SetError;
	this.GetLastErrorCode = Site_GetLastErrorCode;
	this.GetErrorString = Site_GetErrorString;
	this.GetErrorDiagnostic = Site_GetErrorDiagnostic;
	this.SetValidNavigationCommands = Site_SetValidNavigationCommands;  // called when server responds with one that is valid
	this.Init = Site_Init;  // initialize for new activity. Clear all present state. 
	
	// The rte object for this session. Set when frameset calls GetRteApi.
	this.MlcRteApi = null;
	
	// Functions called from Frameset
	this.GetRteApi = Site_GetRteApi;
	this.InitDataModelValues = Site_InitDataModelValues;
	
	// "private" functions
	this.GetDataModelValueToCommit = Site_GetDataModelValueToCommit;
	this.ResetAfterPost = Site_ResetAfterPost;
	this.ClearValidNavigationCommands = Site_ClearValidNavigationCommands;
	
	// "private" data
	this.m_validNavigationCommands = new Array();   // index is <command>, value is either "true" or "false". For choice commands, the index is C,<activityId> (no brackets)
	this.m_errorManager = null; // initialized with the RteApi object
	this.m_scormVersion = null;
}

// Reinitialize object to accept new commands. This removes all current state from memory.
// rteRequired is true if the new activity requires the RTE
function Site_Init( rteRequired )
{
    g_framesetMgr.DebugLog("ApiSite: Init. RteRequired = " + rteRequired);
    this.MlcRteApi = null;
    
    if ( rteRequired )
    {
        // based on scorm version, create api object
        if (this.m_scormVersion == SCORM_2004)
        {   
            this.MlcRteApi = new Rte2004Api(this, new _MS_DataModelParser(), new _MS_Scorm2004_TypeValidator());   
            window.API_1484_11 = this.MlcRteApi;    
        }
        else if (this.m_scormVersion == SCORM_12)
        {
            this.MlcRteApi = new Rte1p2Api(this, new _MS_DataModelParser_1p2(), new _MS_Scorm1p2_TypeValidator());   
            window.API = this.MlcRteApi;    
        }
    }
    else
    {
        window.API_1484_11 = null;
        window.API = null;
    }        
        
    this.m_aValues = new Array();  // holds complete data model
    this.m_aChangedValues = new Array();   // holds only elements that have changed in Values array.
	this.m_validNavigationCommands = new Array();
	if (rteRequired)
        this.m_errorManager = new ErrorMessageManager ( this.m_scormVersion );
    
    // If rte is not required, we do not reinitialize the error manager, as it could be the case where 
    // the current activity just became inactive. In that case, we still need the error manager. 
}

function Site_Terminate()
{
    g_framesetMgr.DebugLog("Terminate");
    
    g_framesetMgr.SetDataModelValues ( this.GetDataModelValueToCommit() );
    g_framesetMgr.Terminate();
    
    for(i in this.m_aChangedValues)
        {
            this.m_aValues[i] = this.m_aChangedValues[i];
        }
        this.m_aChangedValues = new Array();
}

function Site_Commit()
{
    // Get data model information to post
    var strDmValue;
    strDmValue = this.GetDataModelValueToCommit();
    if (g_framesetMgr.CommitDataModel( strDmValue ))
    {
        // If the data was committed to the server... (it might not be if we are in a read-only view)
        // Copy all the changed values into aValues array, since any data access before the post returns 
        // needs to return something reasonable. Then clear out the changed values array. Any values changed between 'now'
        // and when the post returns will get updated on the next post.
        for(i in this.m_aChangedValues)
        {
            this.m_aValues[i] = this.m_aChangedValues[i];
        }
        this.m_aChangedValues = new Array();
    }
}

// Save the name / value pair as a changed value. 
function Site_SetValue(name, value)
{
    this.m_aChangedValues[name] = value;
    g_framesetMgr.DebugLog("SetValue: " + name + " value: " + value);
    
    // If this SetValue invalidated the list of valid navigation commands, then clear that list. For simplicity sake, 
    // the list is cleared if interactions, score, completion_status, progress_measure, exit, or success_status is set. 
    var matches;
    if ( (name.match(/^.*\.interactions\..*/) != null) ||
        ( name.match(/^.*\.score\..*/) != null) || 
        ( name.match(/^.*\.completion_status\..*/) != null) || 
        ( name.match(/^.*\.exit\..*/) != null) || 
        ( name.match(/^.*\.success_status\..*/) != null) || 
        ( name.match(/^.*\.progress_measure\..*/) != null) )
    {
        this.ClearValidNavigationCommands();
    }
}

// Clear the collection of IsMove*Valid commands.
function Site_ClearValidNavigationCommands()
{
    for (i in this.m_validNavigationCommands)
        this.m_validNavigationCommands[i] = null;
}

// The GetValue function will always return the most recent value of the variable, 
// regardless of whether or not the value has been committed. 
// If the site object cannot return a value – preseumably because it does not have such a 
// value in its current data model – then ‘undefined’ is returned.
function Site_GetValue(name)
{
    var getValue = this.m_aChangedValues[name];
    if (this.m_aChangedValues[name] == undefined)
    {
        g_framesetMgr.DebugLog("GetValue from aValues. Name: " + name + " Value: " + this.m_aValues[name]);
        return this.m_aValues[name];  
    }
    else
    {
        g_framesetMgr.DebugLog("GetValue from aChangedValues. Name: " + name + " Value: " + getValue);
        return getValue;
    }
}

// Called to determine if the 'continue' request is valid. If this is the first 
// time it is called, the response will be "unknown" and a request will be sent to 
// the server to get the actual answer. If this the response has already been 
// received from the server, the response will be something else.
function Site_IsContinueRequestValid()
{
    var nextCommandIsValid = this.m_validNavigationCommands[CMD_NEXT];
    if ((nextCommandIsValid != undefined) && (nextCommandIsValid != null) )
    {
        return nextCommandIsValid;
    }
    // Ask the server. Save changed data in order to get correct response.
    g_framesetMgr.SetDataModelValues ( this.GetDataModelValueToCommit() );
    g_framesetMgr.IsNavigationValid( CMD_IS_NAV_VALID, CMD_NEXT );
    
    // Reply 'unknown' for now.
    return "unknown";
}

// Called to determine if the 'previous' request is valid. If this is the first 
// time it is called, the response will be "unknown" and a request will be sent to 
// the server to get the actual answer. If this the response has already been 
// received from the server, the response will be something else.
function Site_IsPreviousRequestValid()
{
    var previousCommandIsValid = this.m_validNavigationCommands[CMD_PREVIOUS];
    if ( (previousCommandIsValid != undefined) && (previousCommandIsValid != null) )
    {
        return previousCommandIsValid;
    }
    // Ask the server. Save changed data in order to get correct response.
    g_framesetMgr.SetDataModelValues ( this.GetDataModelValueToCommit() );
    g_framesetMgr.IsNavigationValid( CMD_IS_NAV_VALID, CMD_PREVIOUS );
    
    // Reply 'unknown' for now.
    return "unknown";
}

// Called to determine if moving to the specified activity is a valid request. If this is the first 
// time it is called, the response will be "unknown" and a request will be sent to 
// the server to get the actual answer. If this the response has already been 
// received from the server, the response will be something else.
function Site_IsChoiceRequestValid(strActivityId)
{
    var choiceCommandIsValid = this.m_validNavigationCommands[CMD_CHOICE + "," + strActivityId];
    if ( (choiceCommandIsValid != undefined) && (choiceCommandIsValid != null) )
    {
        return choiceCommandIsValid;
    }
    // Ask the server. Save changed data in order to get correct response.
    g_framesetMgr.SetDataModelValues ( this.GetDataModelValueToCommit() );
    g_framesetMgr.IsNavigationValid( CMD_IS_CHOICE_VALID , strActivityId );
    
    // Reply 'unknown' for now.
    return "unknown";
}

function Site_ClearError()
{
    m_errorManager.SetError("0");
}

// Set an error condition in the api. This function takes optional additional parameters to substitute into
// the error string. For instance, if errorID 25 corresponds to the string "The {0} parameter is not valid", then
// an additional parameter would be expected to be provided to replace the {0} value.
function Site_SetError( strErrorCode, strErrorMessageId, param1, param2, param3)
{
    this.m_errorManager.SetError(strErrorCode, strErrorMessageId, param1, param2, param3);    
}
 	
function Site_GetLastErrorCode()
{
    return this.m_errorManager.GetLastError();
}

// Return the error message as a string. The api object will call this when the SCO calls GetErrorString, 
// after the api object has verified that it is legal to call this function and that the errorId is within 
// a valid range (in 2004, that’s between 0 and 65536, inclusive).
//
// errorCode is the id of the error that is requested. If this value is an empty string (""), then the 
// error message for the current error state will be returned. In that case, the message that is returned will 
// have any substitutions of messageTokens completed. In other cases, any token markers in the message will be
// replaced with "unknown".
//
// If the errorCode does not correspond to a valid error code for our application, then an empty character 
// string ("") is returned.
function Site_GetErrorString( strErrorId )
{
    return this.m_errorManager.GetErrorString(strErrorId);
}

// Return any additional information about the requested error code. If the errorCode value is 
// an empty string (""), then the message for the current error state will be returned. In that case, 
// the message that is returned will have any substitutions of messageTokens completed. In other cases, 
// any token markers in the message will be replaced with "unknown". If there is no additional error 
// information, an empty character string ("") will be returned.
function Site_GetErrorDiagnostic(errorCode)
{
    return this.m_errorManager.GetDiagnostic( errorCode );
}

function Site_GetRteApi(scormVersion, rteRequired)
{
    if (this.MlcRteApi != null)
        return this.MlcRteApi;
        
    this.m_scormVersion = scormVersion;
    
    this.Init( rteRequired );
    
    return this.MlcRteApi;
}

// Initialize the data model with dmValues. This will reset the m_aValuesChanged to an empty array and 
// initialize the m_aValues array to the values passed in through strDmValues.
// strDmValues is a string of the following form:
// 
// If dmValues is null, then the data model values are all cleared.
function Site_InitDataModelValues( strDmValues )
{
    // Called during initialization of a new activity.
    
    var aDmPairs;   // array of name@Evalue strings
    var aStrNameValue;  // string array with 2 elements: 0 is name, 1 is value
    
    // Clear the master array copy. We do not clear the aChangedValues array because it may contain changes
    // since the last post. (It was reinitialized when the post happened.)
    this.m_aValues = new Array();
    
    // replace @A with @, @L with <, @G with >
    strDmValues = strDmValues.replace(/@G/g, ">").replace(/@L/g, "<").replace(/@A/g, "@");    
    
    // split long string into name@Evalue pairs
    aDmPairs = strDmValues.split("@N");
    
    var i;
    for (i=0; i<aDmPairs.length; i++)
    {
        aStrNameValue = aDmPairs[i].split("@E");
        if (aStrNameValue.length == 2)
        {
            this.m_aValues[aStrNameValue[0]] = aStrNameValue[1];
        }
    }
    
    // There are a few values that are static:
    this.m_aValues["cmi._version"] = "1.0";
    
}

// Return the value of the datamodel string that will be posted. This puts all the values changed 
// since the last commit into the returned string.
function Site_GetDataModelValueToCommit()
{
    var i;
    var strDmValue = ""; // return value
    
    for (i in this.m_aChangedValues)
    {
        // don't ever commit names that end in _count.
        var findCount = /.*_count$/g;
        if (i.match(findCount) == null)
        {
            var name = PrepToPost(i);
            var value = PrepToPost(this.m_aChangedValues[i]);
            strDmValue += name + "@E" + value + "@N";
        }
    }
    return strDmValue;
}

// Prepare the string to be a name or value to be posted to the server.
// Remove any < or > tags from name, in order to make information postable within a form field
function PrepToPost( name )
{
    return name.replace(/@/g, "@A").replace(/</g, "@L").replace(/>/g, "@G");
}

// Reset internal state after data is posted to server
function Site_ResetAfterPost()
{
    this.m_aValues = new Array();
    this.m_aChangedValues = new Array();
}

// Update the list of valid navigation commands based on server response
function Site_SetValidNavigationCommands( strValidCommands )
{
    // The format of strValidCommands is:
    // command@Evalue@N
    // Value will be "true" or "false".
    // In the case of a choice command, the command will be of the form "C,strActivityKey"
    
    // replace @A with @
    strValidCommands = strValidCommands.replace(/@A/g, "@");
    
    var commandPairs; 
    commandPairs = strValidCommands.split("@N");
    
    var i;
    var aStrNameValue;
    for (i=0; i<commandPairs.length; i++)
    {
        aStrNameValue = commandPairs[i].split("@E");
        if (aStrNameValue.length == 2)
        {
            this.m_validNavigationCommands[aStrNameValue[0]] = aStrNameValue[1];
        }
    }
}

// Class to manage the error code, error message and diagnostic message.
function ErrorMessageManager( strScormVer )
{
    // Default parameter to replace the {n} tags in error message format string.
    var defaultParameterValue = " unknown";
	
    var errorDescriptions = new Object();
    
    if ( strScormVer == SCORM_2004 ) 
        InitializeDescriptions_2004();
    else
        InitializeDescriptions_12();

    var lastError = '0';
    var lastErrorMessage = errorDescriptions[lastError];
    var lastDiagnosticMessage = errorDescriptions[lastError];
	
	
	var L_InvalidErrorCode_TXT = "Internal error. The error code {0} is not defined.";
	var L_InvalidErrorMsgId_TXT = "Internal error. The error message id {0} is not defined.";
	
	// Set an error condition
	// errorCode = SCORM error code (e.g., "201")
	// errorMessageId = diagnostic message id  (e.g., "201-1").
	// param1,2,3: parameters to the diagnostic message
    this.SetError = function(errorCode, errorMessageId, param1, param2, param3)
    {
	    lastError = errorCode;
		
	    if (errorMessageId == null || errorMessageId == "")
	    {
		    errorMessageId = errorCode;
	    }
	    lastErrorMessage = errorDescriptions[errorCode];
		
	    if (lastErrorMessage == null)
	    {
		    throw L_InvalidErrorCode_TXT.replace("{0}", errorCode);
	    }
	    lastDiagnosticMessage = errorDescriptions[errorMessageId];
	    if (lastDiagnosticMessage == null)
	    {
		    throw L_InvalidErrorMsgId_TXT.replace("{0}", errorMessageId);
	    }
	    
	    // Possible bug: if the parameter (for example, a data model name) contains '$', 
	    // the replace function will treat $ as special character. 
	    lastDiagnosticMessage = lastDiagnosticMessage.replace(/\{0\}/g, param1 == null ? defaultParameterValue : param1);
	    lastDiagnosticMessage = lastDiagnosticMessage.replace(/\{1\}/g, param2 == null ? defaultParameterValue : param2);
	    lastDiagnosticMessage = lastDiagnosticMessage.replace(/\{2\}/g, param3 == null ? defaultParameterValue : param3);
    }
    
    this.GetLastError = function ()
    {
	    return lastError;
    }
	
    this.GetErrorString = function(errorId)
    {
	    // If the caller wants to retrieve the last error string, return a formatted string
	    if (errorId == lastError)
		    return lastErrorMessage;

	    var msg = errorDescriptions[errorId];
	    if (msg == null) msg = "";
	    return msg;
    }
	
    this.GetDiagnostic = function(errorCode)
    {
	    if (errorCode == null || errorCode == "" || errorCode == lastError)
	    {
		    if (lastDiagnosticMessage != null && lastDiagnosticMessage  != "")
			    return lastDiagnosticMessage;
	    }
	    else
	    {
	        if ((errorCode != null) && (errorCode != ""))
	            return this.GetErrorString(errorCode);
	    }
	    return "";
    }
	
	// Initialize error descriptions for SCORM 2004
	function InitializeDescriptions_2004()
    {
        // NOTE: These strings have 255 max length.
        
    	L_ERROR0_TXT = "No error";
        L_ERROR101_TXT = "General Exception";
        L_ERROR102_TXT = "General Initialization Failure: An error occured while attempting to initialize the communication session.";
        L_ERROR103_TXT = "Session Already Initialized: Calling Initialize is not allowed because the session is already initialized.";
        L_ERROR104_TXT = "Content Instance Terminated: Calling Initialize is not allowed after the session is terminated.";

        L_ERROR111_TXT = "General Termination Failure: An error occurred while attempting to terminate the session.";
        L_ERROR112_TXT = "Cannot Terminate Before Initialization: Calling Terminate is not allowed because the session is not initialized.";
        L_ERROR113_TXT = "Session Already Terminated: Calling Terminate is not allowed because the session is already terminated.";

        L_ERROR122_TXT = "Cannot GetValue Before Initialization: Calling GetValue is not allowed because the session is not yet initialized.";
        L_ERROR123_TXT = "Cannot GetValue After Termination: Calling GetValue is not allowed because the session is already terminated.";
        L_ERROR132_TXT = "Cannot SetValue Before Initialization: Calling SetValue is not allowed because the session is not yet initialized.";
        L_ERROR133_TXT = "Cannot SetValue After Termination: Calling SetValue is not allowed because the session is already terminated.";
        L_ERROR142_TXT = "Cannot Commit Before Initialization: Calling Commit is not allowed because the session is not yet initialized.";
        L_ERROR143_TXT = "Cannot Commit After Termination: Calling Commit is not allowed because the session is already terminated.";

        L_ERROR201_TXT = "General Argument Error";
        L_ERROR2011_TXT = "An empty string must be passed to the {0} function.";
        L_ERROR2012_TXT = "The parameter {0} must be a string.";
        L_ERROR2013_TXT = "The parameter {0} cannot be null.";
        		
        L_ERROR301_TXT = "General GetValue Failure";
        L_ERROR3011_TXT = "The keyword {0} is not supported by data model element {1}.";
        L_ERROR3012_TXT = "The index {0} is expected to be smaller than {1}.";
        L_ERROR3013_TXT = "The collection {0} has not been initialized.";
        		
        L_ERROR351_TXT = "General SetValue Failure";
        L_ERROR3511_TXT = "The keyword {0} is not supported by data model element {1}.";
        L_ERROR3512_TXT = "The index {0} is expected to be equal or smaller than {1}.";
        L_ERROR3513_TXT = "The index of correct responses must be zero for interaction type {0}.";
        L_ERROR3514_TXT = "The correct response pattern {0} is already used by {1}.";
        L_ERROR3515_TXT = "The id {0} must be unique and it is already used by {1}.";
        		
        L_ERROR391_TXT = "General Commit Failure";

        L_ERROR401_TXT = "Undefined Data Model Element";
        L_ERROR4011_TXT = "{0} is not a valid data model element.";
        		
        L_ERROR402_TXT = "Unimplemented Data Model Element";
        		
        L_ERROR403_TXT = "Data Model Element Not Initialized";
        L_ERROR4031_TXT = "The data model element {0} is not initialized.";
        		
        L_ERROR404_TXT = "Read-only Data Model Element";
        L_ERROR4041_TXT = "The data model element {0} is read-only.";
        		
        L_ERROR405_TXT = "Write-only Data Model Element";
        L_ERROR4051_TXT = "The data model element {0} is write-only.";
        		
        L_ERROR406_TXT = "Data Model Element Type Mismatch";
        L_ERROR4061_TXT = "The value {0} is not valid for data model element {1}.";
        	
        L_ERROR407_TXT = "Value Out of Range";
        L_ERROR4071_TXT = "The value {0} is not within the valid range for the {1} data model element.";

        L_ERROR408_TXT = "Dependency Not Established";
        L_ERROR4081_TXT = "The data model element {0} is dependent on {1}, but {1} is not initialized.";
	    
	    errorDescriptions["0"] = L_ERROR0_TXT;
	    errorDescriptions["101"] = L_ERROR101_TXT;
	    errorDescriptions["102"] = L_ERROR102_TXT;
	    errorDescriptions["103"] = L_ERROR103_TXT;
	    errorDescriptions["104"] = L_ERROR104_TXT;
	    errorDescriptions["111"] = L_ERROR111_TXT;
	    errorDescriptions["112"] = L_ERROR112_TXT;
	    errorDescriptions["113"] = L_ERROR113_TXT;
		
	    errorDescriptions["122"] = L_ERROR122_TXT;
	    errorDescriptions["123"] = L_ERROR123_TXT;
	    errorDescriptions["132"] = L_ERROR132_TXT;
	    errorDescriptions["133"] = L_ERROR133_TXT;
	    errorDescriptions["142"] = L_ERROR142_TXT;
	    errorDescriptions["143"] = L_ERROR143_TXT;

	    errorDescriptions["201"] = L_ERROR201_TXT;
	    errorDescriptions["201-1"] = L_ERROR2011_TXT;
	    errorDescriptions["201-2"] = L_ERROR2012_TXT;
	    errorDescriptions["201-3"] = L_ERROR2013_TXT;
		
	    errorDescriptions["301"] = L_ERROR301_TXT;
	    errorDescriptions["301-1"] = L_ERROR3011_TXT;
	    errorDescriptions["301-2"] = L_ERROR3012_TXT;
	    errorDescriptions["301-3"] = L_ERROR3013_TXT;
		
	    errorDescriptions["351"] = L_ERROR351_TXT;
	    errorDescriptions["351-1"] = L_ERROR3511_TXT;
	    errorDescriptions["351-2"] = L_ERROR3512_TXT;
	    errorDescriptions["351-3"] = L_ERROR3513_TXT;
	    errorDescriptions["351-4"] = L_ERROR3514_TXT;
	    errorDescriptions["351-5"] = L_ERROR3515_TXT;
		
	    errorDescriptions["391"] = L_ERROR391_TXT;

	    errorDescriptions["401"] = L_ERROR401_TXT;
	    errorDescriptions["401-1"] = L_ERROR4011_TXT;
		
	    errorDescriptions["402"] = L_ERROR402_TXT;
		
	    errorDescriptions["403"] = L_ERROR403_TXT;
	    errorDescriptions["403-1"] = L_ERROR4031_TXT;
		
	    errorDescriptions["404"] = L_ERROR404_TXT;
	    errorDescriptions["404-1"] = L_ERROR4041_TXT;
		
	    errorDescriptions["405"] = L_ERROR405_TXT;
	    errorDescriptions["405-1"] = L_ERROR4051_TXT;
		
	    errorDescriptions["406"] = L_ERROR406_TXT;
	    errorDescriptions["406-1"] = L_ERROR4061_TXT;
		
	    errorDescriptions["407"] = L_ERROR407_TXT;
	    errorDescriptions["407-1"] = L_ERROR4071_TXT;

	    errorDescriptions["408"] = L_ERROR408_TXT;
	    errorDescriptions["408-1"] = L_ERROR4081_TXT;
    }
    
    // Initialize errors for SCORM 1.2
    function InitializeDescriptions_12()
    {
        var L_ERROR0_TXT = "No error.";
        
        var L_ERROR101_TXT = "General exception error.";
        var L_ERROR1011_TXT = "General exception error. The API instance is already initalized.";
        var L_ERROR1012_TXT = "General exception error. The API instance is already terminated.";
        var L_ERROR1013_TXT = "General exception error. The data model '{0}' is not initialized.";
        var L_ERROR1014_TXT = "General exception error. The type of the data model element {0} cannot be determined.";
        
        var L_ERROR201_TXT = "General argument error.";
        var L_ERROR2011_TXT = "General argument error. Please pass an empty string for '{0}' function.";
        var L_ERROR2012_TXT = "General argument error. '{0}' is not a valid CMI data model element.";
        var L_ERROR2013_TXT = "General argument error. The given data model name must be a string type.";
        var L_ERROR2014_TXT = "General argument error. The value cannot be null or undefined.";
        var L_ERROR2015_TXT = "The index of data model element '{0}' is expected to be smaller than the value of '{1}', which is '{2}'.";
        var L_ERROR2016_TXT = "The index of data model element '{0}' is expected to be smaller than or equal to the value of '{1}', which is '{2}'.";
       
        var L_ERROR202_TXT = "Element cannot have children.";
        var L_ERROR2021_TXT = "The data model element '{0}' cannot have children.";

        var L_ERROR203_TXT = "Element is not an array.";
        var L_ERROR2031_TXT = "The data model element '{0}' cannot have count.";
        
        var L_ERROR301_TXT = "The API instance is not initialized.";

        var L_ERROR402_TXT = "Invalid set value. Data model element is a keyword.";
        var L_ERROR4021_TXT = "Cannot set value for data model element '{0}' because it is a keyword.";
        
        var L_ERROR403_TXT = "Data model element is read-only.";
        var L_ERROR4031_TXT = "The data model element '{0}' is read-only.";
        
        var L_ERROR404_TXT = "Data model element is write-only.";
        var L_ERROR4041_TXT = "The data model element '{0}' is write-only.";

        var L_ERROR405_TXT = "Incorrect data type.";
        var L_ERROR4051_TXT = "The value '{0}' is not valid for data model element '{1}'.";

	    errorDescriptions["0"] = L_ERROR0_TXT;

	    errorDescriptions["101"] = L_ERROR101_TXT;
	    errorDescriptions["101-1"] = L_ERROR1011_TXT;
	    errorDescriptions["101-2"] = L_ERROR1012_TXT;
	    errorDescriptions["101-3"] = L_ERROR1013_TXT;
	    errorDescriptions["101-4"] = L_ERROR1014_TXT;

	    errorDescriptions["201"] = L_ERROR201_TXT;
	    errorDescriptions["201-1"] = L_ERROR2011_TXT;
	    errorDescriptions["201-2"] = L_ERROR2012_TXT;
	    errorDescriptions["201-3"] = L_ERROR2013_TXT;
	    errorDescriptions["201-4"] = L_ERROR2014_TXT;
	    errorDescriptions["201-5"] = L_ERROR2015_TXT;
	    errorDescriptions["201-6"] = L_ERROR2016_TXT;
	    
	    errorDescriptions["202"] = L_ERROR202_TXT;
	    errorDescriptions["202-1"] = L_ERROR2021_TXT;

	    errorDescriptions["203"] = L_ERROR203_TXT;
	    errorDescriptions["203-1"] = L_ERROR2031_TXT;
	    
	    errorDescriptions["301"] = L_ERROR301_TXT;

	    errorDescriptions["402"] = L_ERROR402_TXT;
	    errorDescriptions["402-1"] = L_ERROR4021_TXT;
    
        errorDescriptions["403"] = L_ERROR403_TXT;
        errorDescriptions["403-1"] = L_ERROR4031_TXT;
 		
        errorDescriptions["404"] = L_ERROR404_TXT;
        errorDescriptions["404-1"] = L_ERROR4041_TXT;

        errorDescriptions["405"] = L_ERROR405_TXT;
        errorDescriptions["405-1"] = L_ERROR4051_TXT;
    } 
}