/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// This class is SCORM 2004 RTE api class. 
// To initiate an instance, the caller should specify the apiSite, the parser and the validator.
function Rte2004Api(apiSite, parser, validator)
{
	var ApiSite = apiSite;
	var Parser = parser;
	var Validator = validator;
	
	// The status of current RTE. 
	// It must be one of "NotInitialized", "Running" and "Terminated"
	var RteStatus = "NotInitialized";
	
	this.version = "1.0";
	
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
	
	// Standard API function GetLastError. Returns the last error code.
	this.GetLastError = InternalGetLastError;
	function InternalGetLastError()
	{
		return ApiSite.GetLastErrorCode();
	}
	
	// Standard API function GetErrorString. 
	// Returns the error string according to the given error code.
	this.GetErrorString = InternalGetErrorString;
	function InternalGetErrorString(errorId)
	{
		return ApiSite.GetErrorString(errorId);
	}
	
	// Standard API function GetDiagnostic
	// Returns a diagnostic message for current error, or for specific error code.
	this.GetDiagnostic = InternalGetDiagnostic;
	function InternalGetDiagnostic(errorId)
	{
		return ApiSite.GetErrorDiagnostic(errorId);
	}
	
	// Standard API function Initialize
	this.Initialize = InternalInitialize;
	function InternalInitialize(str)
	{
		if (str != "")
		{
			// general argument error
			InternalSetError("201", "201-1", "Initialize"); 
			return "false";
		}
		
		switch (RteStatus)
		{
			case "Running":
				// already initialized
				InternalSetError("103");
				return "false";
			case "Terminated":
				// It's terminated !
				InternalSetError("104");
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
	
	// Standard API function Terminate
	this.Terminate = InternalTerminate;
	
	function InternalTerminate(str)
	{
		if (str != "")
		{
			// general argument error
			InternalSetError("201", "201-1", "Terminate"); 
			return "false";
		}
		
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// terminated after termination
				InternalSetError("113");
				return "false";
			case "NotInitialized":
				// terminated before initialization
				InternalSetError("112");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		ApiSite.Terminate(str);
		RteStatus = "Terminated";
		InternalClearError();
		return "true";
	}
	
	// Standard API function GetValue
	// Returns the value of given data model name. 
	// Will set error code and return empty string if there is an error. 
	this.GetValue = InternalGetValue;
	function InternalGetValue(name)
	{
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// get value after termination
				InternalSetError("123");
				return "";
			case "NotInitialized":
				// get value before initialization
				InternalSetError("122");
				return "";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		if (typeof(name) != "string")
		{
			// The type of argument must be a string
			InternalSetError("201", "201-2", "name");
			return ""; 
		}
		
		if (name == "")
		{
		    InternalSetError("301");
		    return "";
		}
		
		var handledByNavRequest = false;
		var value = undefined;
		// special treatment for adl data models.
		if (name == "adl.nav.request_valid.continue")
		{
			value = ApiSite.IsContinueRequestValid();
			handledByNavRequest = true;
		}
		else if (name == "adl.nav.request_valid.previous")
		{
			value = ApiSite.IsPreviousRequestValid();
			handledByNavRequest = true;
		}
		else if (name.search(/^adl\.nav\.request_valid\.choice\.\{target=.*\}$/) != -1)
		{
			var target = name.substring('adl.nav.request_valid.choice.{target='.length, name.length - 1);
			value = ApiSite.IsChoiceRequestValid(target);
			handledByNavRequest = true;
		}
		
		if (handledByNavRequest)
		{
			if (value === undefined)
				value = 'unknown';
			InternalClearError();
			return value;
		}
		
		var dataModel = Parser.Parse(name);
		if (dataModel == null)
		{
			// The data model is not found. 
			// The error code still depends on the data model. 
			// For example, the request for cmi.learner_name._version
			// should set error code 301: 
			//   General Get Failure, Data Model Doesn't Support Version.
			// But the request for a cmi.unknown_data_model should set error code 401: 
			//   Data Model Not Defined.
			var kw = SplitDataModelKeywords(name);
			if (kw != null)
			{
				// This data model ends with one of the keywords _version, _children, _count
				// So we need to detect if the former part of the data model name is a valid data model. 
				if ( (!EndsWithKeywords(kw['datamodel'])) && (Parser.Parse(kw['datamodel']) != null))
				{
					// The former part of the data model does not end with 
					// one of the keywords, and the former part is also a 
					// valid data model. 
					// In this case, it should set error code 301. 
					// The given keyword is not supported by this data model. 
					InternalSetError("301", "301-1", kw['keyword'], kw['datamodel']);
					return "";
				}
				// Otherwise, the data model is really not defined. 
				// let it fall through and set error code 401.
			}
			InternalSetError("401", "401-1", name); // Data model is not defined
			return "";
		}
		
		if (dataModel.CanRead == false)
		{
			// cannot read the data model 
			InternalSetError("405", "405-1", name); // Data model is write-only
			return "";
		}
		
		if (dataModel.IndexRequirements != null)
		{
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.comments_from_learner._count"
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
					if (currentCount == '0')
					{
						// The collection is not initialized. Use another error message.
						InternalSetError("301", "301-3", collectionDataModelName); 
						return "";
					}
					else 
					{
						// Data Model Collection Element Request Out Of Range
						InternalSetError("301", "301-2", dataModelIndex, currentCount); 
						return "";
					}
				}
			}
		}

		// should check if the return value is undefined
		try
		{
            if(name == "cmi.completion_status")
            {
                // special processing for completion_status
                progressMeasure = ApiSite.GetValue("cmi.progress_measure");
                completionThreshold = ApiSite.GetValue("cmi.completion_threshold");
                if(progressMeasure == 1.0)
                {
                    value = "completed";
                }
                else if(progressMeasure == 0.0)
                {
                    value = "not attempted";
                }
                else if(progressMeasure == undefined || completionThreshold == undefined)
                {
                    value = ApiSite.GetValue(name);
                }
                else if(progressMeasure >= completionThreshold)
                {
                    value = "completed";
                }
                else
                {
                    value = "incomplete";
                }
            }
            else if(name == "cmi.success_status")
            {
                // special processing for success_status
                scaledScore = ApiSite.GetValue("cmi.score.scaled");
                scaledPassingScore = ApiSite.GetValue("cmi.scaled_passing_score");
                if(scaledPassingScore == undefined)
                {
                    value = ApiSite.GetValue(name);
                }
                else if(scaledScore == undefined)
                {
                    value = "unknown";
                }
                else if(scaledScore >= scaledPassingScore)
                {
                    value = "passed";
                }
                else
                {
                    value = "failed";
                }
            }
            else
            {
			    value = ApiSite.GetValue(name);
			}
		}
		catch (e)
		{
			InternalSetError("301"); //General get data failure
			return "";
		}

		if (value === undefined)
		{
			if (dataModel.DefaultValue === undefined)
			{
				// Data model is not initialized. 
				InternalSetError("403", "403-1", name);
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
	
	// Standard API function SetValue. Sets the value for the data model
	// Returns 'true' or 'false'. 
	// When returning 'false', it also sets error code. 
	this.SetValue = InternalSetValue;
	function InternalSetValue(name, value)
	{
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// set value after termination
				InternalSetError("133");
				return "false";
			case "NotInitialized":
				// set value before initialization
				InternalSetError("132");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		if (typeof(name) != "string")
		{
			// The type of argument must be string
			InternalSetError("201", "201-2", "name");
			return "false"; 
		}

	        if (name == "")
		{
		    InternalSetError("351");
		    return "false";
		}
		
		
		var handledByNavRequest = false;
		// special treatment for adl data models.
		if (name == "adl.nav.request_valid.continue")
		{
			handledByNavRequest = true;
		}
		else if (name == "adl.nav.request_valid.previous")
		{
			handledByNavRequest = true;
		}
		else if (name.search(/^adl\.nav\.request_valid\.choice\.\{target=.*\}$/) != -1)
		{
			handledByNavRequest = true;
		}
		
		if (handledByNavRequest)
		{
			InternalSetError("404", "404-1", name); // Data model is read-only
			return "false";
		}
		
		var dataModel = Parser.Parse(name);
		if (dataModel == null)
		{
			// The data model is not found. 
			// The error code still depends on the data model. 
			// For example, the request for cmi.learner_name._version
			// should set error code 301: 
			//   General Get Failure, Data Model Doesn't Support Version.
			// But the request for a cmi.unknown_data_model should set error code 401: 
			//   Data Model Not Defined.
			var kw = SplitDataModelKeywords(name);
			if (kw != null)
			{
				// This data model ends with one of the keywords _version, _children, _count
				// So we need to detect if the former part of the data model name is a valid data model. 
				if ( (!EndsWithKeywords(kw['datamodel'])) && (Parser.Parse(kw['datamodel']) != null))
				{
					// The former part of the data model does not end with 
					// one of the keywords, and the former part is also a 
					// valid data model. 
					// In this case, it should set error code 351. 
					// The given keyword is not supported by this data model. 
					InternalSetError("351", "351-1", kw['keyword'], kw['datamodel']);
					return "false";
				}
				// Otherwise, the data model is really not defined. 
				// let it fall through and set error code 401.
			}

			InternalSetError("401", "401-1", name) // Data model is not defined
			return "false";
		}
		if (dataModel.CanWrite == false)
		{
			// cannot write the data model 
			InternalSetError("404", "404-1", name); // Data model is read-only
			return "false";
		}
		
		if (dataModel.IndexRequirements != null)
		{
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.comments_from_learner._count"
				// dataModelIndex is like “2”, indicates current index of the 
				// accessing data model.
				var dataModelIndex = dataModel.IndexRequirements[collectionDataModelName];
				var currentCount = InternalGetValue(collectionDataModelName + "._count");
				if (InternalGetLastError() != '0') 
				{
					// cannot get _count. 
					// Although GetValue should have set the error code, 
					// we'd better re-set the error code to indicate that 
					// currently it's Set error, not Get error.
					InternalSetError("351");  // unknown set error
					return "false"; 
				}

				if (parseInt(dataModelIndex) > parseInt(currentCount))
				{
					// Data Model Collection Element Request Out Of Range
					InternalSetError("351", "351-2", dataModelIndex, currentCount); 
					return "false";
				}
				
				if(collectionDataModelName.indexOf("correct_responses") != -1 && 
				   parseInt(dataModelIndex) > 0 &&
				   ApiSite.GetValue(dataModel.DependentModels[1]) == "numeric")
		        {
			        // Data Model Collection Element Request Out Of Range
			        InternalSetError("351", "351-2", dataModelIndex, currentCount); 
			        return "false";
		        }
			}
		}

		// SetValue should check if the dependencies are met.
		if (dataModel.DependentModels != null)
		{
			for (var dm in dataModel.DependentModels)
			{
				if (ApiSite.GetValue(dataModel.DependentModels[dm]) === undefined) 
				{
					// the dependency is not met
					InternalSetError("408", "408-1", name, dataModel.DependentModels[dm]); 
					return 'false';
				}
			}
		}
		
		// SetValue should check if the id is unique within the specified collection of ids
		if (dataModel.UniqueIdName != null)
		{
		    // UniqueIdName is something like cmi.interactions -- a collection within which 
		    // all ids must be unique
		    var idCount = ApiSite.GetValue(dataModel.UniqueIdName + "._count");
		    
		    for (var i=0; i < idCount; i++)
		    {
		        var idElementName = dataModel.UniqueIdName + "." + i + ".id";
		        var id = ApiSite.GetValue(idElementName);
		        
		        if ((idElementName != name) && (id == value))
		        {
		            // the id is not unique
					InternalSetError("351", "351-5", value, idElementName); 
					return 'false';
		        }
		    }
		}

		// IMPORTANT: if the DataType starts with GetValue, it must be like GetValue("cmi.somemodel")
		// The data type is the value of cmi.somemodel.
		// Here the data type is retrieved by ApiSite.GetValue("cmi.somemodel"). 
		// So we'd simply replace the GetValue with ApiSite.GetValue and evaluate it.
		if (dataModel.DataType.search(/^GetValue/) != -1)
		{
			dataModel.DataType = eval('ApiSite.' + dataModel.DataType);
		}
		
		// the value could be some other types like integer etc, 
		// even in standard SCO like PhotoShop packages.
		if (value == null)
		{
			// The value argument must not be null
			InternalSetError("201", "201-3", "value");
			return "false"; 
		}
		else 
		{
			// The value must be number, boolean or string type. 
			var type = typeof(value);
			if (type == "string" || type == "number" || type == "boolean")
			{
				value = value.toString();
			}
			else 
			{
				InternalSetError("406", "406-1", value, name); //Data value mismatch
				return "false";
			}
		}
		
		if (! OnSettingValue(name, value))
		{
			return "false";
		}
		
		if (Validator.Validate(value, dataModel.DataType) == false) 
		{
			InternalSetError("406", "406-1", value, name); //Data value mismatch
			return "false";
		}
		
		if (dataModel.RangeValidator != null && dataModel.RangeValidator(value) == false)
		{
			// validation failed
			InternalSetError("407", "407-1", value, name); // Data value out of range
			return "false";
		}
		
		ApiSite.SetValue(name, value);
		
		if (dataModel.IndexRequirements != null)
		{
			// update the _count data model!
			for (var collectionDataModelName in dataModel.IndexRequirements)
			{
				// collectionDataModelName is like "cmi.comments_from_learner._count"
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

		InternalClearError();
		return 'true';
	}
	
	// Standard API function Commit
	this.Commit = InternalCommit;
	function InternalCommit(str)
	{
		if (str != "")
		{
			// general argument error
			InternalSetError("201", "201-1", "Commit"); 
			return "false";
		}
		switch (RteStatus)
		{
			case "Running":
				break;
			case "Terminated":
				// commit after termination
				InternalSetError("143");
				return "false";
			case "NotInitialized":
				// commit before initialization
				InternalSetError("142");
				return "false";
			default:
				throw "Internal error. The Rte status was not valid.";
		}
		
		InternalClearError();
		ApiSite.Commit();
		return "true";
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
	
	// This function is invoked before a SetValue call really sets the value. 
	// If the function find request is not valid, it is reponsible to set error code, 
	// diagnostic message, and return false. 
	// The caller shall stop the set value request if this function returns false.
	// Otherwise it should return true.
	function OnSettingValue(name, value)
	{
		if (-1 != name.search(/^cmi\.interactions\.(0|[1-9][0-9]*)\.correct_responses\.(0|[1-9][0-9]*)\.pattern$/))
		{
			// this data model is cmi.interactions.n.correct_responses.m.pattern
	
			// remove the cmi.interactions prefix 
			var subname = name.substring("cmi.interactions.".length, name.length);
			var dotIndex = subname.search(/\./);
			var interactionIndex = subname.substring(0, dotIndex);
			
			subname = subname.substring(interactionIndex.length + '.correct_responses.'.length, subname.length);
			dotIndex = subname.search(/\./);
			var correctResponsesIndex = subname.substring(0, dotIndex);
			var iCorrectResponsesIndex = parseInt(correctResponsesIndex);

			// if cmi.interactions.n.type is true-false, the index m must always be zero.
			var interactionType = InternalGetValue("cmi.interactions." + interactionIndex + ".type");
			if (InternalGetLastError() != "0")
				throw "Internal error on retrieving cmi.interactions.n.type.";
				
			switch (interactionType)
			{ 
			case "true-false":
				// For true-false type, the correct responses index should always be zero.
				if (correctResponsesIndex != '0')
				{
					InternalSetError("351", "351-3", interactionType);
					return false;
				}
				break;
			case "likert":
				// For likert type, the correct responses index should always be zero.
				if (correctResponsesIndex != '0')
				{
					InternalSetError("351", "351-3", interactionType);
					return false;
				}
				break;
			case "choice":
				// The choice must not be identical. 
				// For example, the value of cmi.interactions.0.correct_responses.0.pattern
				// must not be equal to cmi.interactions.0.correct_responses.1.pattern
				var correctResponsesCount = InternalGetValue("cmi.interactions." + interactionIndex + ".correct_responses._count");
				if (InternalGetLastError() != "0")
					throw "Internal error on retrieving cmi.interactions.n.correct_responses._count.";
				correctResponsesCount = parseInt(correctResponsesCount); 
				
				for (var i = 0; i < correctResponsesCount; i ++)
				{
					if (i == iCorrectResponsesIndex)
					{
						// Do not compare it with itself
						continue;
					}
					var otherModelName = "cmi.interactions." + interactionIndex + ".correct_responses." + i + ".pattern";
					var otherPattern = InternalGetValue(otherModelName);
					if (InternalGetLastError() != "0")
						throw "Internal error on retrieving cmi.interactions.n.correct_responses.m.pattern.";
					if (CompareMultipleChoice(otherPattern, value) == 0)
					{
						// The pattern already exists.
						InternalSetError("351", "351-4", value, otherModelName);
						return false;
					}
				}
				break;
			case "sequencing":
				// duplicate values are not allowed. 
				// For example, the value of cmi.interactions.0.correct_responses.0.pattern
				// must not be equal to cmi.interactions.0.correct_responses.1.pattern
				var correctResponsesCount = InternalGetValue("cmi.interactions." + interactionIndex + ".correct_responses._count");
				if (InternalGetLastError() != "0")
					throw "Internal error on retrieving cmi.interactions.n.correct_responses._count.";
				correctResponsesCount = parseInt(correctResponsesCount); 
				
				for (var i = 0; i < correctResponsesCount; i ++)
				{
					if (i == iCorrectResponsesIndex)
					{
						// Do not compare it with itself
						continue;
					}
					var otherModelName = "cmi.interactions." + interactionIndex + ".correct_responses." + i + ".pattern";
					var otherPattern = InternalGetValue(otherModelName);
					if (InternalGetLastError() != "0")
						throw "Internal error on retrieving cmi.interactions.n.correct_responses.m.pattern.";
					if (otherPattern == value)
					{
						// The pattern already exists.
						InternalSetError("351", "351-4", value, otherModelName);
						return false;
					}
				}
				break;
			}
		}
		// by default.
		return true;
	}
	
	// Compare two multiple-choice value. If they are identical, this function returns 0. 
	// The values are strings separated with [,]. The order doesn't matter. 
	// So the result is that "str1[,]str2" is equal to "str2[,]str1".
	function CompareMultipleChoice(value1, value2)
	{
		if (value1 == value2) return 0;
		var sub1 = value1.split('[,]');
		var sub2 = value2.split('[,]');
		
		if ((sub1 == null || sub1.length == 0) && (sub2 == null || sub2.length == 0) )
			return 0;

		if (sub1.length != sub2.length)
			return sub1.length - sub2.length;
		
		// a comparison delegate
		var compareString = function(s1, s2)
		{
			if (s1 > s2) return 1;
			if (s1 < s2) return -1;
			return 0;
		}
		
		// sort and compare
		sub1.sort(compareString);
		sub2.sort(compareString);
		
		for (var i = 0; i < sub1.length; i ++)
		{
			var res = compareString(sub1[i], sub2[i]);
			if (res != 0) return res;
		}
		return 0;
	}


}