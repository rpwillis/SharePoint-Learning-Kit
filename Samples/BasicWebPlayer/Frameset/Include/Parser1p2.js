/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

function _MS_DataModelParser_1p2()
{

var CmiDataModels = new Array();
var RegexpDataModelDelegates = new Array();

InitializeDataModels();

this.Parse = function (dataModelName)
{
    if (typeof(dataModelName) != 'string')
        throw "Internal error. The data model name must be a string.";
    
    if (dataModelName.substring(0, 4) == "cmi.")
    {
        if (CmiDataModels[dataModelName] != null) 
            return CmiDataModels[dataModelName];

		// Then search each regular expression
		for (var i in RegexpDataModelDelegates)
		{
			if (RegexpDataModelDelegates[i].RegExp.test(dataModelName))
			{
				return RegexpDataModelDelegates[i].Parser(dataModelName);
			}
		}
        
        return null;
    }
    
    // by default, a data model is read-write
    return new DataModel(true, true, false);
}

function DataModel(canRead, canWrite, isKeyword, dataType, indexRequirements , defaultValue)
{
    this.CanRead = canRead;
    this.CanWrite = canWrite;
    this.IsKeyword = isKeyword;
    this.DataType = dataType;
    this.IndexRequirements = indexRequirements;
    this.DefaultValue = defaultValue;
}

// This structure stores the regular expression and the customized parser function.
function RegExpValidatorPair(regexp, parserDelegate)
{
	this.RegExp = regexp;
	this.Parser = parserDelegate;
}

function ParserDataModel_Cmi_Objectives(name)
{
	var subname = name.substring("cmi.objectives.".length, name.length);
	
	var dotIndex = subname.search(/\./);
	// get the index number. It should be an integer
	var myIndex = subname.substring(0, dotIndex);
	var indexRequest = new Object();
	indexRequest['cmi.objectives'] = myIndex;
	
	subname = subname.substring(dotIndex + 1, subname.length);
	switch (subname)
	{
	case 'id':
	    return new DataModel(true, true, false, 'CmiIdentifier', indexRequest, '');
	case 'score._children':
	    return new DataModel(true, false, true, 'CmiString255', indexRequest, 'raw,min,max');
	case 'score.raw':
	case 'score.min':
	case 'score.max':
	    return new DataModel(true, true, false, 'CmiDecimal0-100OrBlank', indexRequest, '');
	case 'status':
	    return new DataModel(true, true, false, 'CmiStatus', indexRequest, 'not attempted');
	}
	return null;
}

function ParserDataModel_Cmi_Interactions(name)
{
	var subname = name.substring("cmi.interactions.".length, name.length);
	
	var dotIndex = subname.search(/\./);
	// get the index number. It should be an integer
	var myIndex = subname.substring(0, dotIndex);
	var indexRequest = new Object();
	indexRequest['cmi.interactions'] = myIndex;
	
	subname = subname.substring(dotIndex + 1, subname.length);
	switch (subname)
	{
	case 'id':
	    return new DataModel(false, true, false, "CmiIdentifier", indexRequest);
	case 'time':
	    return new DataModel(false, true, false, "CmiTime", indexRequest);
	case 'type':
	    return new DataModel(false, true, false, "CmiInteractionType", indexRequest);
	case 'weighting':
	    return new DataModel(false, true, false, "CmiDecimal", indexRequest);
	case 'student_response':
	    return new DataModel(false, true, false, "GetValue('" + 
	            "cmi.interactions." + myIndex + ".type')", 
	            indexRequest);
	case 'result':
	    return new DataModel(false, true, false, "CmiResult", indexRequest);
	case 'latency':
	    return new DataModel(false, true, false, "CmiTimespan", indexRequest);

	case 'objectives._count':
	    return new DataModel(true, false, true, "CmiInteger", indexRequest, '0');

	case 'correct_responses._count':
	    return new DataModel(true, false, true, "CmiInteger", indexRequest, '0');
	}
	
	// case objectives.n.id
	if (subname.search(/^objectives\.(0|[1-9][0-9]*)\.id$/) != -1)
	{
	    var objIndex = subname.substring('objectives.'.length, subname.length);
	    objIndex = objIndex.substring(0, objIndex.indexOf("."));
	    
	    indexRequest['cmi.interactions.' + myIndex + '.objectives'] = objIndex;
	    return new DataModel(false, true, false, "CmiIdentifier", indexRequest);
	}
	
	// case correct_responses.m.pattern
	if (subname.search(/^correct_responses\.(0|[1-9][0-9]*)\.pattern$/) != -1)
	{
	    var objIndex = subname.substring('correct_responses.'.length, subname.length);
	    objIndex = objIndex.substring(0, objIndex.indexOf("."));
	    
	    indexRequest['cmi.interactions.' + myIndex + '.correct_responses'] = objIndex;
	    return new DataModel(false, true, false, "GetValue('" + 
	            "cmi.interactions." + myIndex + ".type')", 
	            indexRequest);
	}
	
	return null;
}


function InitializeDataModels()
{
    CmiDataModels['cmi.core._children'] = new DataModel(true, false, true, "CmiString255", null, 'student_id,student_name,lesson_location,credit,lesson_status,entry,score,total_time,lesson_mode,exit,session_time');
    CmiDataModels['cmi.core.student_id'] = new DataModel(true, false, false, "CmiIdentifier");
    CmiDataModels['cmi.core.student_name'] = new DataModel(true, false, false, "CmiString255");
    CmiDataModels['cmi.core.lesson_location'] = new DataModel(true, true, false, "CmiString255", null, "");
    CmiDataModels['cmi.core.credit'] = new DataModel(true, false, false, "CmiCredit", null, "credit");
    CmiDataModels['cmi.core.lesson_status'] = new DataModel(true, true, false, "CmiStatus", null, "not attempted");
    CmiDataModels['cmi.core.entry'] = new DataModel(true, false, false, "CmiEntry", null, "ab-inito");
    CmiDataModels['cmi.core.score._children'] = new DataModel(true, false, true, "CmiString255", null, "raw,min,max");
    CmiDataModels['cmi.core.score.raw'] = new DataModel(true, true, false, "CmiDecimal0-100OrBlank", null, "");
    CmiDataModels['cmi.core.score.min'] = new DataModel(true, true, false, "CmiDecimal0-100OrBlank", null, "");
    CmiDataModels['cmi.core.score.max'] = new DataModel(true, true, false, "CmiDecimal0-100OrBlank", null, "");
    CmiDataModels['cmi.core.total_time'] = new DataModel(true, false, false, "CmiTimespan", null, "0000:00:00.00");
    CmiDataModels['cmi.core.lesson_mode'] = new DataModel(true, false, false, "CmiLessonMode", null, "normal");
    CmiDataModels['cmi.core.exit'] = new DataModel(false, true, false, "CmiExit");
    CmiDataModels['cmi.core.session_time'] = new DataModel(false, true, false, "CmiTimespan", null, "0000:00:00.00");
    CmiDataModels['cmi.suspend_data'] = new DataModel(true, true, false, "CmiString4096", null, "");
    CmiDataModels['cmi.launch_data'] = new DataModel(true, false, false, "CmiString4096", null, "");
    CmiDataModels['cmi.comments'] = new DataModel(true, true, false, "CmiString4096", null, "");
    CmiDataModels['cmi.comments_from_lms'] = new DataModel(true, false, false, "CmiString4096", null, "");
    CmiDataModels['cmi.objectives._children'] = new DataModel(true, false, true, "CmiString255", null, "id,score,status");
    CmiDataModels['cmi.objectives._count'] = new DataModel(true, false, true, "CmiCount", null, "0");

	RegexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.objectives\.(0|[1-9][0-9]*)\.(id|score\.(_children|raw|min|max)|status)$/, ParserDataModel_Cmi_Objectives));
    CmiDataModels['cmi.student_data._children'] = new DataModel(true, false, true, "CmiString255", null, "mastery_score,max_time_allowed,time_limit_action");
    CmiDataModels['cmi.student_data.mastery_score'] = new DataModel(true, false, false, "CmiDecimal");
    CmiDataModels['cmi.student_data.max_time_allowed'] = new DataModel(true, false, false, "CmiTimespan");
    CmiDataModels['cmi.student_data.time_limit_action'] = new DataModel(true, false, false, "CmiTimeLimitAction");

    CmiDataModels['cmi.student_preference._children'] = new DataModel(true, false, true, "CmiString255", null, "audio,language,speed,text");
    CmiDataModels['cmi.student_preference.audio'] = new DataModel(true, true, false, "CmiIntegerNegativeOneTo100", null, "0");
    CmiDataModels['cmi.student_preference.language'] = new DataModel(true, true, false, "CmiString255", null, "");
    CmiDataModels['cmi.student_preference.speed'] = new DataModel(true, true, false, "CmiIntegerNegative100To100", null, "0");
    CmiDataModels['cmi.student_preference.text'] = new DataModel(true, true, false, "CmiIntegerNegativeOneToOne", null, "0");
    
    CmiDataModels['cmi.interactions._children'] = new DataModel(true, false, true, "CmiString255", null, "id,objectives,time,type,correct_responses,weighting,student_response,result,latency");
    CmiDataModels['cmi.interactions._count'] = new DataModel(true, false, true, "CmiCount", null, "0");
	RegexpDataModelDelegates.push(new RegExpValidatorPair(/^cmi\.interactions\.(0|[1-9][0-9]*)\./, ParserDataModel_Cmi_Interactions));
}

}
