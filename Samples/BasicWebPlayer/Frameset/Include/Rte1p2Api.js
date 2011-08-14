/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */


function Rte1p2Api(apiSite, parser, validator)
{
    var ApiSite = apiSite;
    var Parser = parser; 
	var Validator = validator;
    var isDirty = false;
    
    var RteStatus = "NotInitialized";

    this.LMSGetValue = InternalGetValue;
    function InternalGetValue(name)
    {
    	switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// get value after termination
				InternalSetError("101-2");
				return "";
			case "NotInitialized":
				// get value before initialization
				InternalSetError("301");
				return "";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		if (typeof(name) != "string")
		{
			// The type of argument must be a string
			InternalSetError("201", "201-3");
			return ""; 
		}
		
		if (name == "")
		{
		    InternalSetError("201", "201-2", name);
		    return "";
		}
		
		var dataModel = Parser.Parse(name);
		if (dataModel == null)
		{
		    // The data model is not found. 
			// The error code still depends on the name of the requested element
			// For example, the request for cmi.learner_name._children
			//      should set error code 202: Element cannot have children
			// But the request for a cmi.unknown_data_model should set error code 201: 
			//   Invalid argument error
			var kw = SplitDataModelKeywords(name);
			if (kw != null)
			{
				// This data model ends with one of the keywords _children or _count
				// So we need to detect if the former part of the data model name is a valid data model. 
				if ( (!EndsWithKeywords(kw['datamodel'])) && (Parser.Parse(kw['datamodel']) != null))
				{
					// The former part of the data model does not end with 
					// one of the keywords, and the former part is also a 
					// valid data model. 
					// In this case, it should set error code depending on which keyword is being asked for 
					if (kw['keyword'] == "_count")
					{
					    InternalSetError("203", "", name);
					    return "";
					}
					else if (kw['keyword'] == "_children")
					{
					    InternalSetError("202", "", name);
					    return "";
					}
					
					// Otherwise, let it fall through and set the generic error
				}
			}
			InternalSetError("201", "201-2", name);
		    return "";
		}
		
		if (dataModel.CanRead == false)
		{
	        // data model is write-only
            InternalSetError("404", "404-1", name);
		    return "";
		}
		
		if (dataModel.IndexRequirements != null)
		{
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.objectives._count"
				// dataModelIndex is like “2”, indicates current index of the 
				// accessing data model.
				var dataModelIndex = dataModel.IndexRequirements[collectionDataModelName];
				var currentCount = InternalGetValue(collectionDataModelName + "._count");
				if (InternalGetLastError() != '0') 
				{
					// cannot get _count. The error code should have been set by 
					// GetValue(cmi.*._count) call.
					return ""; 
				}

				if (parseInt(dataModelIndex) >= parseInt(currentCount))
				{
					// Data Model Collection Element Request Out Of Range
					InternalSetError("201", "201-5", name, collectionDataModelName + "._count", currentCount); 
					return "";
				}
			}
		}

		var value = undefined;
		try
		{
			value = ApiSite.GetValue(name);
		}
		catch (e)
		{
			InternalSetError("101"); //General get data failure
			return "";
		}
		
		if (value === undefined)
		{
			if (dataModel.DefaultValue === undefined)
			{
				// Data model is not initialized. 
				InternalSetError("101", "101-3", name);
				return "";
			}
			else 
			{
				// return default value instead 
				InternalClearError();
				return dataModel.DefaultValue;
			}
		}
		else 
		{
			InternalClearError();
			return value;
		}
    }
    
    this.LMSSetValue = InternalSetValue;
    function InternalSetValue(name, value)
    {
    	switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// set value after termination
				InternalSetError("101-2");
				return "false";
			case "NotInitialized":
				// set value before initialization
				InternalSetError("301");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		if (typeof(name) != "string")
		{
			// The type of argument must be a string
			InternalSetError("201", "201-3");
			return "false"; 
		}
		
		if (name == "")
		{
		    InternalSetError("201", "201-2", name);
		    return "false";
		}
		
		var dataModel = Parser.Parse(name);
		if (dataModel == null)
		{
		    InternalSetError("201", "201-2", name);
		    return "false";
		}
		
		if (dataModel.IsKeyword)
		{
		    InternalSetError("402", "402-1", name);
		    return "false";
		}

		if (dataModel.CanWrite == false)
		{
		    InternalSetError("403", "403-1", name);
		    return "false";
		}
		
		if (dataModel.IndexRequirements != null)
		{
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.objectives._count"
				// dataModelIndex is like “2”, indicates current index of the 
				// accessing data model.
				var dataModelIndex = dataModel.IndexRequirements[collectionDataModelName];
				var currentCount = InternalGetValue(collectionDataModelName + "._count");
				if (InternalGetLastError() != '0') 
				{
					// cannot get _count. The error code should have been set by 
					// GetValue(cmi.*._count) call.
					return "false"; 
				}

				if (parseInt(dataModelIndex) > parseInt(currentCount))
				{
					// Data Model Collection Element Request Out Of Range
					InternalSetError("201", "201-6", name, collectionDataModelName + "._count", currentCount); 
					return "false";
				}
			}
		}
		
		// retrieve the real data type. It might be the value of other data model. 
		if (dataModel.DataType.search(/^GetValue/) != -1)
		{
			var tmpType= eval('ApiSite.' + dataModel.DataType);
			if (tmpType != null)
			{
			    dataModel.DataType = tmpType;
			}
			else 
			{
			    // This data model is dependent on other data model but it was not initialized.
			    // So the result is 
    			InternalSetError("101", "101-4", name);
	    		return "false"; 
			}
		}
		
		var type = typeof(value);
		if (value == null)
		{
			InternalSetError("201", "201-4");
			return "false"; 
		}
		else if (type != 'string' && type != 'number' && type != 'boolean')
		{
		    InternalSetError("405", "405-1", value, name);
		    return "false";
		}
		else 
		{
		    value = value.toString();
	    }
	    
	    if (Validator.Validate(value, dataModel.DataType) == false) 
		{
			InternalSetError("405", "405-1", value, name); //Data value mismatch
			return "false";
		}
		
		ApiSite.SetValue(name, value);

		if (dataModel.IndexRequirements != null)
		{
			// update the _count data model!
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.objectives._count"
				// dataModelIndex is like “2”, indicates current index of the 
				// accessing data model.
				var dataModelIndex = dataModel.IndexRequirements[collectionDataModelName];
				var currentCount = InternalGetValue(collectionDataModelName + "._count");
				var numCurrentCount = parseInt(currentCount);
				var numDataModelIndex = parseInt(dataModelIndex);
				if (numDataModelIndex == numCurrentCount)
				{
					ApiSite.SetValue(collectionDataModelName + "._count", (numDataModelIndex + 1).toString());
				}
			}
		}
				
		isDirty = true;
		InternalClearError();
		return "true";
    }
    
    this.LMSInitialize = InternalInitialize;
    function InternalInitialize(str)
    {
        if (str != "")
		{
			InternalSetError("201", "201-1", "LMSInitialize"); 
			return "false";
		}
		
		switch (RteStatus)
		{
			case "Running":
				// already initialized
				InternalSetError("101", "101-1");
				return "false";
			case "Terminated":
				// It's terminated !
				InternalSetError("101", "101-2");
				return "false";
			case "NotInitialized":
				break;
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		RteStatus = "Running";
		InternalClearError();
		return "true";
	}
    
    this.LMSFinish = InternalFinish;
    function InternalFinish(str)
    {
        if (str != "")
		{
			// general argument error
			InternalSetError("201", "201-1", "LMSFinish"); 
			return "false";
		}
		
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// terminated after termination
				InternalSetError("101", "101-2");
				return "false";
			case "NotInitialized":
				// terminated before initialization
				InternalSetError("301");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		ApiSite.Terminate(str);
		RteStatus = "Terminated";
		InternalClearError();
		return "true";
    }
    
    this.LMSCommit = InternalCommit;
    function InternalCommit(str)
    {
		if (str != "")
		{
			// general argument error
			InternalSetError("201", "201-1", "LMSCommit"); 
			return "false";
		}
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// commit after termination
				InternalSetError("101", "101-2");
				return "false";
			case "NotInitialized":
				// commit before initialization
				InternalSetError("301");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		InternalClearError();
		ApiSite.Commit();
		isDirty = false;
		return "true";
	}
    
    this.LMSGetLastError = InternalGetLastError;
    function InternalGetLastError()
    {
		return ApiSite.GetLastErrorCode();
    }
    
	function InternalClearError()
	{
		InternalSetError("0");
	}
	
	// Internal function for set error code and error message parameters.
	// Centralize the error handling for easier update.
	// The errorMessageId could be different with errorCode because some error code 
	// covers dozens of error cases, and each case have their own error message definition.
	// The parameters are used to format the error message string.
	function InternalSetError(errorCode, errorMessageId, param1, param2, param3)
	{
		ApiSite.SetError(errorCode, errorMessageId, param1, param2, param3);
	}
    
    this.LMSGetErrorString = InternalGetErrorString;
    function InternalGetErrorString(errorId)
    {
		return ApiSite.GetErrorString(errorId);
    }
    
    
    this.LMSGetDiagnostic = InternalGetDiagnostic;
    function InternalGetDiagnostic(errorId)
    {
		return ApiSite.GetErrorDiagnostic(errorId);
    }
    
    // Detect if a data model name ends with one of the keywords _version, _children, and _count
	function EndsWithKeywords(dataModelName)
	{
		return dataModelName.search(/\._(version|children|count)$/) != -1;
	}

	// Detect if a data model name ends with one of the keywords 
	// _count, _children, _version. If it ends with one of them, 
	// the function returns an object that has the split result. 
	// 
	// For example, if the data model parameter is "cmi._version", 
	// the return value is an object with obj['keyword'] = '_version', 
	// and obj['datamodel'] = 'cmi'. 
	// 
	// The return value is null if the data model doesn't ends with 
	// the keywords.
	function SplitDataModelKeywords(dataModelName)
	{
		var keyword = dataModelName.match(/\._(version|children|count)$/);
		if (keyword == null)
		{
			return null;
		}
		
		keyword = keyword[0];

		var obj = new Object();
		obj['keyword'] = keyword.substring(1, keyword.length);
		obj['datamodel'] = dataModelName.substring(0, dataModelName.length - keyword.length);
		return obj;
	}

}