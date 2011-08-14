/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SCORM 2004 Data Model Parser 
function _MS_DataModelParser()
{
	// Initialization of the parser object. 
	
	// This collection stores the pre-defined data models.
	var dataModels = new Array();
	
	// This collection stores the RegEx-Customized Parse function pair. 
	// If the data model passed the test of the regular expression, 
	// the data model is handled by the given function.
	var regexpDataModelDelegates = new Array();
	
	// common-used data models
	var readonlyDataModel = new DataModel(true, false);
	var countDataModel = new DataModel(true, false, 'integer', null, null, null, '0');
	
	dataModels["cmi._version"] = new DataModel(true, false, 'string', null, null, null, '1.0');
	dataModels["cmi.completion_status"] = new DataModel(true, true, 'cmi_completion_status', null, null, null, 'unknown');
	dataModels["cmi.completion_threshold"] = readonlyDataModel;
	dataModels["cmi.credit"] = new DataModel(true, false, 'string', null, null, null, 'credit');
	dataModels["cmi.entry"] = new DataModel(true, false, 'string', null, null, null, '');
	dataModels["cmi.exit"] = new DataModel(false, true, 'cmi_exit');

	dataModels["cmi.comments_from_learner._count"] = countDataModel;
	dataModels["cmi.comments_from_learner._children"] = new DataModel(true, false, 'string', null, null, null, 'comment,location,timestamp');
	regexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.comments_from_learner\.(0|[1-9][0-9]*)\.(comment|location|timestamp)$/, ParserDataModel_Cmi_CommentsFromLearner));

	dataModels["cmi.comments_from_lms._count"] = countDataModel;
	dataModels["cmi.comments_from_lms._children"] = new DataModel(true, false, 'string', null, null, null, 'comment,location,timestamp');
	regexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.comments_from_lms\.(0|[1-9][0-9]*)\.(comment|location|timestamp)$/, ParserDataModel_Cmi_CommentsFromLMS));
	
	dataModels["cmi.interactions._children"] = new DataModel(true, false, 'string', null, null, null, 'id,type,objectives,timestamp,correct_responses,weighting,learner_response,result,latency,description');
	dataModels["cmi.interactions._count"] = countDataModel;
	regexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.interactions\.(0|[1-9][0-9]*)\./, ParserDataModel_Cmi_Interaction));

	dataModels["cmi.launch_data"] = readonlyDataModel;
	dataModels["cmi.learner_id"] = new DataModel(true, false, 'long_identifier', null, null, null, '');
	dataModels["cmi.learner_name"] = new DataModel(true, false, 'string', null, null, null, '');
	
	dataModels["cmi.learner_preference._children"] = new DataModel(true, false, 'string', null, null, null, 'audio_level,delivery_speed,language,audio_captioning');
	dataModels["cmi.learner_preference.audio_level"] = new DataModel(true, true, 'real', null, PositiveAndZero, null, '1');
	dataModels["cmi.learner_preference.audio_captioning"] = new DataModel(true, true, 'audio_captioning', null, null, null, '0');
	dataModels["cmi.learner_preference.language"] = new DataModel(true, true, 'language_type_allow_empty', null, null, null, '');
	dataModels["cmi.learner_preference.delivery_speed"] = new DataModel(true, true, 'real', null, PositiveAndZero, null, '1');
	
	dataModels["cmi.location"] = new DataModel(true, true, 'string');
	dataModels["cmi.max_time_allowed"] = readonlyDataModel;
	dataModels["cmi.mode"] = new DataModel(true, false, 'string', null, null, null, 'normal');
	
	dataModels["cmi.objectives._children"] =  new DataModel(true, false, 'string', null, null, null, 'id,score,success_status,completion_status,description,progress_measure');
	dataModels["cmi.objectives._count"] = countDataModel;
	regexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.objectives\.(0|[1-9][0-9]*)\./, ParserDataModel_Cmi_Objectives));
	
	dataModels["cmi.progress_measure"] = new DataModel(true, true, 'real', null, ZeroToOne);
	dataModels["cmi.scaled_passing_score"] = new DataModel(true, false, 'real', null, NegativeOneToOne);
	
	dataModels["cmi.score._children"] = new DataModel(true, false, 'string', null, null, null, 'scaled,min,max,raw');
	dataModels["cmi.score.scaled"] = new DataModel(true, true, 'real', null, NegativeOneToOne);
	dataModels["cmi.score.raw"] = new DataModel(true, true, 'real');
	dataModels["cmi.score.max"] = new DataModel(true, true, 'real');
	dataModels["cmi.score.min"] = new DataModel(true, true, 'real');
   
	dataModels["cmi.session_time"] = new DataModel(false, true, 'time_interval');
	dataModels["cmi.success_status"] = new DataModel(true, true, 'success_status', null, null, null, 'unknown');
	dataModels["cmi.suspend_data"] = new DataModel(true, true, 'string');
	dataModels["cmi.time_limit_action"] = new DataModel(true, false, 'time_limit_action', null, null, null, 'continue,no message');
	dataModels["cmi.total_time"] = new DataModel(true, false, 'time_interval', null, null, null, 'PT0H0M0S');
	
	dataModels["adl.nav.request"] = new DataModel(true, true, 'adl_nav_request', null, null, null, '_none_');
	
	
	// range validation for float value. 
	function PositiveAndZero(str)
	{
		return parseFloat(str) >= 0;
	}
	// range validation for float value. 
	function ZeroToOne(str)
	{
		var val = parseFloat(str);
		return val >= 0 && val <= 1;
	}
	// range validation for float value. 
	function NegativeOneToOne(str)
	{
		var val = parseFloat(str);
		return val >= -1 && val <= 1;
	}
	
	// This structure stores the regular expression and the customized parser function.
	function RegExpValidatorPair(regexp, parserDelegate)
	{
		this.RegExp = regexp;
		this.Parser = parserDelegate;
	}

	// This struct describes all properties of a data model. 
	// The instance is always returned by Parse function. 
	function DataModel(canRead, canWrite, dataType, dependentModels, rangeValidator, indexRequirements, defaultValue, uniqueIdName)
	{
		// If this data model is readable by SCO (some data model are write-only)
		// GetValue should set error code 405 if this flag is false. 405 = write only
		this.CanRead = canRead; 

		// If this data model is writable by SCO
		// SetValue should set error code 404 if this flag is false. 404 = readonly
		this.CanWrite = canWrite; 
		
		// The data type of this data model. 
		// It must be one of the values specified in the TypeValidators class. 
		// For exmaple, it can be 'string', 'loc_string', 'time_interval', 'state_cmi_exit', etc.
		// The SetValue function is responsible to check the data type. 
		// If it failed, SetValue should set error code 406. 
		// GetValue function doesn't care about this property.
		this.DataType = dataType;
		
		// Advanced validator for this data model. 
		// It's a delegate function that returns boolean value for a string prameter. 
		// For example, it validates if a string "0.7" is between 0.0 and 1.0.
		// SetValue should set error code 407 if this valiation failed. 
		this.RangeValidator = rangeValidator;
		
		// A collection of the dependent model name. 
		// For example, the data model "cmi.interactions.3.correct_reponses.2.pattern" 
		// is dependent on the data model "cmi.interactions.3.type". 
		// SetValue should set error code 408 if the dependent model is not previously set. 
		this.DependentModels = dependentModels;
		
		// This is a hashtable that describe how the data model has index dependencies. 
		// For example, when the user sets data model "cmi.comments_from_learner.2.comment", 
		// SetValue function should check if "cmi.comments_from_learner._count" is 2 or greater. 
		// The same thing goes for GetValue, but GetValue requires the count to be 3 or greater. 
		// SetValue function should return 351 (general set error, index out of range), and 
		// GetValue function should return 301 (general get error, index out of range) if the 
		// index is out of range. 
		// 
		// The hashtable defines the object model and the least request on its _count.
		// For example, "cmi.comments_from_learner.2.comment" data model finally creates 
		// an entry in the hash table like IndexRequirements['cmi.comments_from_learner'] = '2';
		// There are some cases that the object model requires multiple index conditions to be met. 
		// For example, "cmi.interactions.2.objectives.1" creates two entries like: 
		// IndexRequirements['cmi.interactions'] = '2' and 
		// IndexRequirements['cmi.interactions2.objectives'] = '1'. 
		this.IndexRequirements = indexRequirements;
		
		// The default value for this data model. 
		// This attributes is only used by GetValue. 
		// If GetValue found this data model is undefined by ApiSite, the default value will be returned. 
		this.DefaultValue = defaultValue;
		
		// If defined, this is the name of the collection within which an id value must be unique.
		// The attribute is only used by SetValue in collections with ids (objectives).
		// The value is something like "cmi.objectives" to verify that cmi.objectives.n.id is a unique 
		// id.
		this.UniqueIdName = uniqueIdName;
	} 
	
	// This function parses a data model name (for example, "cmi._version")
	// to a DataModel structure (defined as below)
	// The DataModel structure denotes all the nessesary properties of a data model, 
	// for example, if it's readable, if it's writable, how to validate it, etc. 
	// For more information, please see the DataModel definition.
	this.Parse = function (name)
	{
	    // ADL has confirmed that the conformance tests incorrectly expect names starting 
		// with 'foo' to be an error condition, so we include it in these test in order to return 
		// null. They intend to update their tests with the next version, at which point 'foo' 
		// in the following regex and the else statement should be removed. See PS bug 1079.
		
		if (name.search(/^(cmi|adl|foo)\./) != -1)
		{
			// First search the predefined data model collection.
			if (dataModels[name] != null)
				return dataModels[name];
			
			// Then search each regular expression
			for (var i in regexpDataModelDelegates)
			{
				if (regexpDataModelDelegates[i].RegExp.test(name))
				{
					return regexpDataModelDelegates[i].Parser(name);
				}
			}
			
			// this data model is not supported by our system.
			return null;
		}
		else if (name == 'cmi' || name == 'adl' || name == 'foo')
		{
		    return null;
		}
		else {
			// this is not a reserved data model. 
			return new DataModel(true, true, 'string');
		}
	} 
	
	// Parse function for cmi.comments_from_learner.n.*
	function ParserDataModel_Cmi_CommentsFromLearner(name)
	{
		var subname = name.substring("cmi.comments_from_learner.".length, name.length);
		
		var dotIndex = subname.search(/\./);
		// get the index number. It should be an integer
		var myIndex = subname.substring(0, dotIndex);
		var indexRequest = new Object();
		indexRequest['cmi.comments_from_learner'] = myIndex;

		var type = null;
		if (subname.search(/comment/) != -1)
		{
			// comment is a localized string
			type = 'localized_string';
		}
		else if (subname.search(/location/) != -1)
		{
			// location is just a string
			type = 'string';
		}
		else if (subname.search(/timestamp/) != -1)
		{
			// timestamp is a timestamp
			type = 'time';
		}
		else 
		{
			throw 'Internal error - Unknown data model: ' + name;
		}
		
		return new DataModel(true, true, type, null, null, indexRequest);
	}
	
	// Parse function for cmi.comments_from_lms.n.*
	function ParserDataModel_Cmi_CommentsFromLMS(name)
	{
		var subname = name.substring("cmi.comments_from_lms.".length, name.length);
		
		var dotIndex = subname.search(/\./);
		// get the index number. It should be an integer
		var myIndex = subname.substring(0, dotIndex);
		var indexRequest = new Object();
		indexRequest['cmi.comments_from_lms'] = myIndex;

		var type = null;
		if (subname.search(/comment/) != -1)
		{
			// comment is a localized string
			type = 'localized_string';
		}
		else if (subname.search(/location/) != -1)
		{
			// location is just a string
			type = 'string';
		}
		else if (subname.search(/timestamp/) != -1)
		{
			// timestamp is a timestamp
			type = 'time';
		}
		else 
		{
			throw 'Internal error - Unknown data model: ' + name;
		}
		
		return new DataModel(true, false, type, null, null, indexRequest);
	}
	
  
	// This function parses data models like 
	// cmi.interactions.n.(id|type) etc
	function ParserDataModel_Cmi_Interaction(name)
	{
		var subname = name.substring("cmi.interactions.".length, name.length);
		// now subname equals to n.id, n.type or n.objectives.k._count, etc
		
		// get the index number. It should be an integer
		var dotIndex = subname.search(/\./);
		var interactionIndex = subname.substring(0, dotIndex);
		var indexRequest = new Object();
		indexRequest['cmi.interactions'] = interactionIndex;
		
		// default dependency for most data models.
		var depends = new Array();
		depends.push("cmi.interactions." + interactionIndex +  ".id");

		subname = subname.substring(dotIndex + 1, subname.length);
		if (subname == 'id')
		{
			// cmi.interactions.n.id
			return new DataModel(true, true, 'long_identifier', null, null, indexRequest, null, null);
		}
		else if (subname == 'type')
		{
			// cmi.interactions.n.type
			return new DataModel(true, true, 'interaction_type', depends, null, indexRequest);
		}
		else if (subname == 'timestamp')
		{
			// cmi.interactions.n.timestamp
			// it has the dependency on cmi.interactions.n.id
			return new DataModel(true, true, 'time', depends, null, indexRequest);
		}
		else if (subname == 'weighting')
		{
			// cmi.interactions.n.timestamp
			// it has the dependency on cmi.interactions.n.id
			return new DataModel(true, true, 'real', depends, null, indexRequest);
		}
		else if (subname == 'learner_response')
		{
			// cmi.interactions.n.learner_response
			// it has the dependency on cmi.interactions.n.id AND cmi.interactions.n.type
			// The type is the value of cmi.interactions.n.type
			depends.push("cmi.interactions." + interactionIndex +  ".type");
			
            // The type of this data model is (value of cmi.interactions.n.type) + "-lr" // means learner_responses
			return new DataModel(true, true, 'GetValue("cmi.interactions.' + interactionIndex + '.type") + "-lr"', depends, null, indexRequest);
		}
		else if (subname == 'result')
		{
			// cmi.interactions.n.result
			// it has the dependency on cmi.interactions.n.id 
			return new DataModel(true, true, 'interaction_result', depends, null, indexRequest);
		}
		else if (subname == 'latency')
		{
			// cmi.interactions.n.latency
			// it has the dependency on cmi.interactions.n.id 
			return new DataModel(true, true, 'time_interval', depends, null, indexRequest);
		}
		else if (subname == 'description')
		{
			// cmi.interactions.n.description
			// it has the dependency on cmi.interactions.n.id 
			return new DataModel(true, true, 'localized_string', depends, null, indexRequest);
		}
		
		// now we are sure subname must be objectives.* or correct_responses.* 
		if (subname.search(/^objectives\./) != -1)
		{
			if (subname == 'objectives._count')
			{
				return new DataModel(true, false, 'integer', null, null, null, '0');
			}
			else if (subname.search(/^objectives.(0|[1-9][0-9]*)\.id$/) != -1)
			{
				// cmi.interactions.n.objectives.k.id, should also check the index cmi.interactions.n.objectives._count
				subname = subname.substring('objectives.'.length, subname.length);
				var idx = subname.search(/\./);
				var objIndex = subname.substring(0, idx);
				var objArrayName = 'cmi.interactions.' + interactionIndex + '.objectives';
				indexRequest[objArrayName] = objIndex;
				
				return new DataModel(true, true, 'long_identifier', depends, null, indexRequest, null, objArrayName);
			}
		}
		else if (subname.search(/^correct_responses\./) != -1)
		{
			if (subname == 'correct_responses._count')
			{
				return new DataModel(true, false, 'integer', null, null, null, '0');
			}
			else if (subname.search(/^correct_responses.(0|[1-9][0-9]*)\.pattern$/) != -1)
			{
				// cmi.interactions.n.correct_responses.k.pattern
				// should also check the index cmi.interactions.n.correct_responses._count
				subname = subname.substring('correct_responses.'.length, subname.length);
				var idx = subname.search(/\./);
				var objIndex = subname.substring(0, idx);
				indexRequest['cmi.interactions.' + interactionIndex + '.correct_responses'] = objIndex;
				
				// It has the dependency on cmi.interactions.n.id AND cmi.interactions.n.type
				// The type is the value of cmi.interactions.n.type
				depends.push("cmi.interactions." + interactionIndex +  ".type");

                // The type of this data model is (value of cmi.interactions.n.type) + "-cr" // means correct_responses
				return new DataModel(true, true, 'GetValue("cmi.interactions.' + interactionIndex + '.type") + "-cr"', depends, null, indexRequest);
			}
		}
		
		// the subname is not valid.
		return null;
	}
	
	
	// Parse function for cmi.objectives.n.*
	function ParserDataModel_Cmi_Objectives(name)
	{
		var subname = name.substring("cmi.objectives.".length, name.length);
		
		var dotIndex = subname.search(/\./);
		// get the index number. It should be an integer
		var objectiveIndex = subname.substring(0, dotIndex);
		var objectiveArrayName = "cmi.objectives";
		var indexRequest = new Object();
		indexRequest[objectiveArrayName] = objectiveIndex;

		subname = subname.substring(dotIndex + 1, subname.length);

		var depends = new Array();
		depends.push("cmi.objectives." + objectiveIndex +  ".id");
		
		if (subname == 'id')
		{
			return new DataModel(true, true, 'long_identifier', null, null, indexRequest, null, objectiveArrayName);
		}
		else if (subname == 'score._children')
		{
			// location is just a string
			return new DataModel(true, false, 'string', null, null, indexRequest, 'scaled,raw,min,max');
		}
		else if (subname == 'score.scaled')
		{
			return new DataModel(true, true, 'real', depends, NegativeOneToOne, indexRequest);
		}
		else if (subname == 'score.raw')
		{
			return new DataModel(true, true, 'real', depends, null, indexRequest);
		}
		else if (subname == 'score.min')
		{
			return new DataModel(true, true, 'real', depends, null, indexRequest);
		}
		else if (subname == 'score.max')
		{
			return new DataModel(true, true, 'real', depends, null, indexRequest);
		}
		else if (subname == 'success_status')
		{
			return new DataModel(true, true, 'success_status', depends, null, indexRequest, 'unknown');
		}
		else if (subname == 'completion_status')
		{
			return new DataModel(true, true, 'cmi_completion_status', depends, null, indexRequest, 'unknown');
		}
		else if (subname == 'progress_measure')
		{
			return new DataModel(true, true, 'real', depends, ZeroToOne, indexRequest);
		}
		else if (subname == 'description')
		{
			return new DataModel(true, true, 'string', depends, null, indexRequest);
		}
		
		return null;
	}  

}
