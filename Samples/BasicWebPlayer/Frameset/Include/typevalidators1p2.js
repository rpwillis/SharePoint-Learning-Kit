/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// This file contains functions for SCORM 1.2 data type validation.
function _MS_Scorm1p2_TypeValidator()
{
	// Initalize a hashtable for the data type and data validator function.
	var validators = new Object();
	validators['CmiString255'] = IsCmiString255;
	validators['CmiString4096'] = IsCmiString4096;
	validators['CmiIdentifier'] = IsCmiIdentifier;
	validators['CmiCredit'] = IsCmiCredit;
	validators['CmiStatus'] = IsCmiStatus;
	validators['CmiLessonMode'] = IsCmiLessonMode;
	validators['CmiEntry'] = IsCmiEntry;
	validators['CmiDecimal0-100OrBlank'] = IsCmiDecimal0To100OrBlank;
	validators['CmiDecimal'] = IsCmiDecimal;
	validators['CmiTimespan'] = IsCmiTimespan;
	validators['CmiTime'] = IsCmiTime;
	validators['CmiExit'] = IsCmiExit;
	validators['CmiTimeLimitAction'] = IsCmiTimeLimitAction;
	validators['CmiIntegerNegativeOneTo100'] = IsCmiIntegerNegativeOneTo100;
	validators['CmiIntegerNegative100To100'] = IsCmiIntegerNegative100To100;
	validators['CmiIntegerNegativeOneToOne'] = IsCmiIntegerNegativeOneToOne;
	validators['CmiInteractionType'] = IsCmiInteractionType;
	validators['true-false'] = IsTrueFalse;
	validators['choice'] = IsChoice;
	validators['fill-in'] = IsFillIn;		
	validators['likert'] = IsLikert; 
	validators['matching'] = IsMatching; 
	validators['performance'] = IsPerformance; 
	validators['sequencing'] = IsSequencing; 
	validators['numeric'] = IsCmiDecimal; 
	validators['CmiResult'] = IsCmiResult;

	// Check if the value is specific data type.
	this.Validate = function (value, type)
	{
		if (validators[type] == null)
		{
			// Don't need globalization because it should never happen.
			throw "Internal error - the type " + type + " is unknown.";
		}
		if (typeof(value) != "string")
			return false; 
			
		return validators[type](value);
	}
	
	// Returns a collection of supported data types
	this.AllTypes = function()
	{
		var allTypes = new Array();
		for (var t in validators)
		{
			allTypes.push(t);
		}
		return allTypes;
	}
	
	function IsCmiString(str)
	{
	    return true;
	}

	function IsCmiString255(str)
	{
		return str.length <= 255;
	}
	
	function IsCmiString4096(str)
	{
		return str.length <= 4096;
	}
	
	function IsCmiIdentifier(str)
	{
		if (str.length > 255) 
		    return false;
		return str.search(/^[A-Za-z0-9\-_:]+$/) != -1;
	}
	
	function IsCmiCredit(str)
	{
		return str == "credit" || str == "no-credit";
	}
	
	function IsCmiStatus(str)
	{
		return str.search(/^(passed|completed|failed|incomplete|browsed|not attempted)$/) != -1; 
	}

	function IsCmiLessonMode(str)
	{
		return str.search(/^(browse|normal|review)$/) != -1; 
	}	
	
	function IsCmiEntry(str)
	{
		return str == "ab-initio" || str == "resume" || str == ""; 
	}
	
	function IsCmiExit(str)
	{
		return str == "time-out" || str == "suspend" || str == "logout" || str == ""; 
	}
	
	function IsCmiTimespan(str)
	{
	    // HHHH:MM:SS.SS
	    // Hour: 2-4 digits. The decimal part of seconds is optional (0-2 digits).
	    return str.search( /^\d{2,4}:\d\d:\d\d(\.\d{1,2})?$/ ) != -1;
	}
	
	function IsCmiTime(str)
	{
	    // HH:MM:SS[.SS]
	    // Hour: 2 digits. The decimal part of seconds is optional (0-2 digits).
	    
   	    if (str.search( /^\d\d:\d\d:\d\d(\.\d{1,2})?$/ ) != -1){
	        var timeparts = str.split(':');
	        if(timeparts[0] < 24 && timeparts[1] < 60 && timeparts[2] < 60)
	            return true;
	    }
	    return false;
	}	
	
	function IsCmiDecimal(str)
	{
	    //return str.search( /^[-+]?(0|([1-9]\d*))(\.\d+)?$/ ) != -1; 
	    return str.search(/\d+/) != -1 && str.search( /^[-+]?((0*)|([1-9]\d*))(\.\d+)?$/ ) != -1;
	    
	}
	
	function IsCmiDecimal0To100OrBlank(str)
	{
	    if (str == "") 
	        return true;
	    if (!IsCmiDecimal(str))
	        return false;
	    var val = parseFloat(str);
	    return val >= 0 && val <= 100;
	}
	
	function IsCmiTimeLimitAction(str)
	{
	    return str.search(/^(exit|continue),(no )?message$/) != -1;
	}
	
	function IsCmiInteger(str)
	{
	    return str.search( /^[-+]?((0*)|([1-9]\d*))$/ ) != -1; 
	}
	
	function IsCmiIntegerNegativeOneTo100(str)
	{
	    if (!IsCmiInteger(str))
	        return false;
	    var val = parseInt(str);
	    return val >= -1 && val <= 100;
	}
	
	function IsCmiIntegerNegative100To100(str)
	{
	    if (!IsCmiInteger(str))
	        return false;
	    var val = parseInt(str);
	    return val >= -100 && val <= 100;
	}

	function IsCmiIntegerNegativeOneToOne(str)
	{
	    if (!IsCmiInteger(str))
	        return false;
	    var val = parseInt(str);
	    return val >= -1 && val <= 1;
	}
	
	function IsCmiInteractionType(str)
	{
	    return str == "true-false" || 
	        str == "choice" || 
	        str == "fill-in" || 
	        str == "matching" || 
	        str == "performance" || 
	        str == "sequencing" || 
	        str == "likert" || 
	        str == "numeric";
	}
	
	// for true-false type
	function  IsTrueFalse(str)
	{
	    str = str.charAt(0);
	    return str == '0' || str == '1' || str == 't' || str == 'f';
	}

    function IsChoice(str)
    {
        // format like "{1,a,b,c}" or"1,a,b,c"
        // One and only one comma, others are alphanumeric characters
        return str.search(/^\{[0-9a-z](,[0-9a-z])*\}$/) != -1 
            || str.search(/^[0-9a-z](,[0-9a-z])*$/) != -1;
    }
    
    function IsFillIn(str)
    {
        // alphanumeric string
        return str.search(/^[\d\w\s]*$/) != -1;
    }
    
    function IsLikert(str)
    {
        // only one character 0-9 or a-z
        return str.search(/^[0-9a-z]?$/) != -1;
    }
    
    function IsIdentifierPair(str)
    {
        // format like "012,564" or "act,zob"
        return str.search(/^[0-9a-zA-Z]+,[0-9a-zA-Z]+$/) != -1;
    }
    
    function IsMatching(str)
    {
        // format like "1,t.4,1" or surrounded by {}
        return str.search(/^[0-9a-z]\.[0-9a-z](,[0-9a-z]\.[0-9a-z])*$/) != -1  || 
            str.search(/^\{[0-9a-z]\.[0-9a-z](,[0-9a-z]\.[0-9a-z])*\}$/) != -1 ;
    }
    
    function IsSequencing(str)
    {
        // format like "1,a,9,z"
        return str.search(/^[0-9a-z](,[0-9a-z])*$/) != -1;
    }
    
    function IsPerformance(str)
    {
        // alphanumeric string limited to 255 characters
        return IsCmiString255(str);
    }
    
    function IsCmiResult(str)
    {
        return IsCmiDecimal(str) || 
            str == "correct" ||
            str == "wrong" ||
            str == "unanticipated" ||
            str == "neutral" ;
    }
}