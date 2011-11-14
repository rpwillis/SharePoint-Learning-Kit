/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// This file contains functions for SCORM 2004 data type validation.
function _MS_Scorm2004_TypeValidator()
{
	// Initalize a hashtable for the data type and data validator function.
	var validators = new Object();
	validators['string'] = IsCharacterString;
	validators['integer'] = IsInteger;
	validators['real'] = IsReal;
	validators['short_identifier'] = IsShortIdentifier;
	validators['long_identifier'] = IsLongIdentifier;
	validators['time'] = IsTime;
	validators['time_interval'] = IsTimeInterval;
	validators['language_type'] = IsLanguageType;
	validators['language_type_allow_empty'] = IsLanguageTypeOrEmpty;
	validators['localized_string'] = IsLocalizedStringType;
	validators['cmi_exit'] = IsCmiExit;
	validators['cmi_completion_status'] = IsCmiCompletionStatus;
	validators['interaction_type'] = IsInteractionTypes;
	validators['interaction_result'] = IsInteractionResults;
	validators['true-false-lr'] = IsTrueFalse;
	validators['true-false-cr'] = IsTrueFalse;
	validators['choice-lr'] = IsChoice;
	validators['choice-cr'] = IsChoice;
	validators['fill-in-lr'] = IsFillIn_LearnerResponse;		
	validators['fill-in-cr'] = IsFillIn_CorrectPattern;		
	validators['long-fill-in-lr'] = IsLongFillIn; 
	validators['long-fill-in-cr'] = IsLongFillIn; 
	validators['likert-lr'] = IsLikert; 
	validators['likert-cr'] = IsLikert; 
	validators['matching-lr'] = IsMatching_LearnerResponse; 
	validators['matching-cr'] = IsMatching_CorrectPattern; 
	validators['performance-lr'] = IsPerformance_LearnerResponse; 
	validators['performance-cr'] = IsPerformance_CorrectPattern; 
	validators['sequencing-lr'] = IsSequencing; 
	validators['sequencing-cr'] = IsSequencing; 
	validators['numeric-lr'] = IsReal; 
	validators['numeric-cr'] = IsMinMaxNumeric; 
	validators['other-lr'] = IsCharacterString; 
	validators['other-cr'] = IsCharacterString; 
	validators['audio_captioning'] = IsAudioCaptioning; 
	validators['success_status'] = IsSuccessStatus; 
	validators['time_limit_action'] = IsTimeLimitAction; 
	validators['adl_nav_request'] = IsAdlNavRequest; 
	validators['true_false_unknown'] = IsTrueFalseUnknown; 

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

	// Check if a variable is character string, i.e., a string in JavaScript
	function IsCharacterString(str)
	{
		return true;
	}

	// Check if a variable is a integer. The variable must be a JavaScript number and cannot have decimal part.
	function IsInteger(strNum)
	{
		return (strNum.search(/^[+-]?\d+$/) != -1);
	}

	// Check if a variable is a real value, i.e., a JavaScript number.
	function IsReal(strNum)
	{
		return (strNum.search(/^[+-]?\d*\.?\d+(e[+-]?\d+)?$/i) != -1);
	}

	// Check if a variable is a valid SCORM time type. 
	// The variable must be a JavaScript string type and match the SCORM time format.
	function IsTime(str)
	{
		// the format is YYYY[-MM[-DD[Thh[:mm[:ss[.s[TZD]]]]]]]
		var reg = /^\d{4}(-(0[1-9]|1[0-2])(-(0[1-9]|[1-2]\d|3[0-1])(T([01]\d|2[0-3])(:[0-5]\d(:[0-5]\d(\.\d{1,2}(Z|([\+\-]([0-1]\d|2[0-3])(:[0-5]\d)?))?)?)?)?)?)?)?$/;
		if (!reg.test(str))
		    return false;
		    
	    var year = parseInt(str.substring(0, 4));
	    // year is limited to [1970, 2038]
	    if (year < 1970 || year > 2038) return false;
	    
	    var month = str.length > 5 ? str.substring(5, 7) : null;
	    if (month == null) return true;
	    if (month.substring(0, 1) == "0") 
	        month = month.substring(1, 2);
	    month = parseInt(month);
	    
	    var date = str.length > 8 ? parseInt(str.substring(8, 10)) : null;
	    if (date == null) return true;
	    
	    // deal with common year.
	    if (date == 31 && (month == 2 || month == 4 || month == 6 || month == 9 || month == 11)) 
	        return false; 
	        
	    if (month == 2)
	    {
	        if (date > 29) 
	        {
	            return false;
	        }
	        else if (date == 29)
	        {
	            return (year % 4 == 0) && ((year % 400 == 0) || (year % 100 != 0));
	        }
	    }
	    return true;
	}

	// Check if a variable is a valid SCORM timeinterval type. 
	// The variable must be a JavaScript string type and match the SCORM timeinterval format.
	function  IsTimeInterval(str)
	{
		if (str == 'P' || str == 'PT' || str.substr(str.length - 1, 1) == 'T')
			return false;
		// the format is P[yY][mM][dD][T[hH][mM][s[.s]S]]
		var reg = /^P(\d+Y)?(\d+M)?(\d+D)?(T(\d+H)?(\d+M)?(\d+(.\d{1,2})?S)?)?$/;
		return reg.test(str);
	}

    // cmi.learner_preference.language data model element allows empty string,  
    // but in other cases, the language type in localized_string is not allowed to be empty.
    function IsLanguageTypeOrEmpty(str)
    {
        return str == "" || IsLanguageType(str);
    }
    
	// Check if a variable is a valid language_type of SCORM 2004. 
	// The variable must be a strign and match the language_type format.
	function IsLanguageType(str)
	{
		// i, x are reserverd language type. 
		// The language types are copied from http://www.w3.org/WAI/ER/IG/ert/iso639.htm
		var reg = /^(i|x|abk|ace|ach|ada|aar|afh|afr|afa|aka|akk|alb|sqi|ale|alg|tut|amh|apa|ara|arc|arp|arn|arw|arm|hye|art|asm|ath|map|ava|ave|awa|aym|aze|nah|ban|bat|bal|bam|bai|bad|bnt|bas|bak|baq|eus|bej|bem|ben|ber|bho|bih|bik|bin|bis|bra|bre|bug|bul|bua|bur|mya|bel|cad|car|cat|cau|ceb|cel|cai|chg|cha|che|chr|chy|chb|chi|zho|chn|cho|chu|chv|cop|cor|cos|cre|mus|crp|cpe|cpf|cpp|cus|ces|cze|dak|dan|del|din|div|doi|dra|dua|dut|nla|dum|dyu|dzo|efi|egy|eka|elx|eng|enm|ang|esk|epo|est|ewe|ewo|fan|fat|fao|fij|fin|fiu|fon|fra|fre|frm|fro|fry|ful|gaa|gae|gdh|glg|lug|gay|gez|geo|kat|deu|ger|gmh|goh|gem|gil|gon|got|grb|grc|ell|gre|kal|grn|guj|hai|hau|haw|heb|her|hil|him|hin|hmo|hun|hup|iba|ice|isl|ibo|ijo|ilo|inc|ine|ind|ina|ine|iku|ipk|ira|gai|iri|sga|mga|iro|ita|jpn|jav|jaw|jrb|jpr|kab|kac|kam|kan|kau|kaa|kar|kas|kaw|kaz|kha|khm|khi|kho|kik|kin|kir|kom|kon|kok|kor|kpe|kro|kua|kum|kur|kru|kus|kut|lad|lah|lam|oci|lao|lat|lav|ltz|lez|lin|lit|loz|lub|lui|lun|luo|mac|mak|mad|mag|mai|mak|mlg|may|msa|mal|mlt|man|mni|mno|max|mao|mri|mar|chm|mah|mwr|mas|myn|men|mic|min|mis|moh|mol|mkh|lol|mon|mos|mul|mun|nau|nav|nde|nbl|ndo|nep|new|nic|ssa|niu|non|nai|nor|nno|nub|nym|nya|nyn|nyo|nzi|oji|ori|orm|osa|oss|oto|pal|pau|pli|pam|pag|pan|pap|paa|fas|per|peo|phn|pol|pon|por|pra|pro|pus|que|roh|raj|rar|roa|ron|rum|rom|run|rus|sal|sam|smi|smo|sad|sag|san|srd|sco|sel|sem|scr|srr|shn|sna|sid|bla|snd|sin|sit|sio|sla|ssw|slk|slo|slv|sog|som|son|wen|nso|sot|sai|esl|spa|suk|sux|sun|sus|swa|ssw|sve|swe|syr|tgl|tah|tgk|tmh|tam|tat|tel|ter|tha|bod|tib|tig|tir|tem|tiv|tli|tog|ton|tru|tsi|tso|tsn|tum|tur|ota|tuk|tyv|twi|uga|uig|ukr|umb|und|urd|uzb|vai|ven|vie|vol|vot|wak|wal|war|was|cym|wel|wol|xho|sah|yao|yap|yid|yor|zap|zen|zha|zul|zun|AA|AB|AF|AM|AR|AS|AY|AZ|BA|BE|BG|BH|BI|BN|BO|BR|CA|CO|CS|CY|DA|DE|DZ|EL|EN|EO|ES|ET|EU|FA|FI|FJ|FO|FR|FY|GA|GD|GL|GN|GU|HA|HI|HR|HU|HY|IA|IE|IK|IN|IS|IT|IW|JA|JI|JW|KA|KK|KL|KM|KN|KO|KS|KU|KY|LA|LN|LO|LT|LV|MG|MI|MK|ML|MN|MO|MR|MS|MT|MY|NA|NE|NL|NO|OC|OM|OR|PA|PL|PS|PT|QU|RM|RN|RO|RU|RW|SA|SD|SG|SH|SI|SK|SL|SM|SN|SO|SQ|SR|SS|ST|SU|SV|SW|TA|TE|TG|TH|TI|TK|TL|TN|TO|TR|TS|TT|TW|UK|UR|UZ|VI|VO|WO|XH|YO|ZH|ZU|ak|an|av|ae|bm|nb|bs|ch|ce|ny|za|cu|cv|kw|cr|dv|ee|ff|lg|ki|ht|he|hz|ho|io|ig|id|iu|jv|kr|kv|kg|kj|lb|li|lu|gv|mh|nv|nd|nr|ng|se|nn|oj|os|pi|sc|ii|ty|ug|ve|wa|yi|ady|arg|ast|aus|btk|byn|nob|bos|cmc|chp|chk|nwc|crh|hrv|dar|day|dgr|nld|myv|fur|gla|gba|nds|gor|gwi|hat|hit|hmn|ido|smn|inh|ile|gle|kbd|xal|krc|csb|kmb|kos|lim|jbo|dsb|lua|smj|lus|mkd|mnc|mdr|glv|mdf|nap|nia|nog|sme|phi|rap|sat|sas|scc|srp|iii|sgn|sms|den|snk|sma|tai|tet|tlh|tpi|tkl|tup|tvl|udm|hsb|wln|ypk|znd|fil|mwl|scn)(-[a-z0-9-]{2,8})?$/i;
		return reg.test(str);
	}

	// Internal function used by IsLocalizedStringType. 
	// Find a case_matters delimiter in a localized_string_type. Like {case_matters=XXX}.
	// The return value is null if the delimiter is not found. 
	// It returns an array of delimiter matches (the length is always 1). 
	// This function doesn't check if the delimiter is valid or not.
	function FindCaseMatters(str)
	{
		var reg = /^\{case_matters=.*?\}/;
		return reg.exec(str);
	}

	// Internal function usd by IsFillIn. 
	// Find a lang delimiter in a localized_string_type. Like {lang=en-us}
	// The return value is null if the delimiter is not found. 
	// It returns an array of delimiter matches (the length is always 1). 
	// This function doesn't check if the delimiter is valid or not.
	function FindLang(str)
	{
		var reg = /^\{lang=.*?\}/;
		return reg.exec(str);
	}
	
	// Internal function used by IsFillIn. 
	// Find a order_matters delimiter in a fill-in type. Like {order_matters=false}.
	// The return value is null if the delimiter is not found. 
	// It returns an array of delimiter matches (the length is always 1). 
	// This function doesn't check if the delimiter is valid or not.
	function FindOrderMatters(str)
	{
		var reg = /^\{order_matters=.*?\}/;
		return reg.exec(str);
	}

	// Check if a string is SCORM localized_string_type.
	function  IsLocalizedStringType(str)
	{
		// Deal with common cases to avoid reg-expression check everytime
		// If a string has delimiters, it must start with "{", longer than 4 characters "{x=}"
		// and must have character = and '}'
		if (str.length < 4 || str.substring(0,1) != '{' || str.search(/\=.*\}/) == -1)
			return true; // no delimiters found. It's a valid localized string.

		// Find language type and case_matters, if any
		var lang_match = FindLang(str);
		
		if (lang_match == null)
		{
			// no delimiters found. It's a valid localized string.
			return true;
		}
		else // lang_match != null)
		{
			// get the language type "XXX" from "{lang=XXX}"
			var langType = lang_match[0].substring('{lang='.length, lang_match[0].length - 1); // }
			return IsLanguageType(langType);
		}
	}
	
	// Used by cmi.exit  
	function  IsCmiExit(str)
	{
		// The cmi.exit must be one of the following values (could be empty)
		return str.search(/^(time-out|suspend|logout|normal)?$/) != -1;
	}

	// used by cmi.completion_status
	function  IsCmiCompletionStatus(str)
	{
		return str.search(/^(completed|incomplete|not attempted|unknown)$/) != -1;
	}
	
	// for cmi.interactions.n.type
	function  IsInteractionTypes(str)
	{
		return str.search(/^(true-false|choice|fill-in|long-fill-in|likert|matching|performance|sequencing|numeric|other)$/) != -1;
	}
	
	// for cmi.interactions.n.result
	function  IsInteractionResults(str)
	{
		if (IsReal(str)) return true;
		return str.search(/^(correct|incorrect|unanticipated|neutral)$/) != -1;
	}
	
	// for true-false type
	function  IsTrueFalse(str)
	{
		return str == 'true' || str == 'false';
	}
	
	// <localized_string_type>[,]<localized_string_type>
	function IsFillIn_LearnerResponse(str)
	{
	    if (str == "") return true;
	    
		var substrings = str.split('[,]');
		for (var si in substrings)
		{
			if (!IsLocalizedStringType(substrings[si]))
			{
				// failed validation for some sub string.
				return false;
			}
		}
    }
	
	// for fill-in type
	// The format is {order_matters=xx}{case_matters=xx}<array of localized string separated with [,]>
	function IsFillIn_CorrectPattern(str)
	{
		// Deal with common cases to avoid reg-expression check everytime
		// If a string has delimiters, it contain "{", longer than 4 characters "{x=}"
		// and must have character = and '}'
		if (str.length < 4 || str.search(/\{.*\=.*\}/) == -1)
			return true; // no delimiters found. It must be valid.

		// Find order_matters type and case_matters, if any
		var order_match = FindOrderMatters(str);
		var case_matters_match = FindCaseMatters(str);
		
		if (order_match == null && case_matters_match == null)
		{
			// no delimiters found. The function should continue check delimiter [,]
		}
		else if (order_match != null)
		{
			if ( order_match[0].search(/^\{order_matters=(true|false)\}$/) != -1 )
			{
				// trim the {order_matters=XXX} and check case matters.
				str = str.substring(order_match[0].length, str.length);
				
				case_matters_match = FindCaseMatters(str);
				if (case_matters_match != null && 
					(case_matters_match[0].search(/^\{case_matters=(true|false)\}$/) == -1))
				{
					return false; // case_matters failed validation
				}
				
				if (case_matters_match != null)
				{
					// trim the {case_matters=xxx}
					str = str.substring(case_matters_match[0].length, str.length);
				}
			}
			else 
			{
				return false; // invalid order_matters
			}
		}
		else // case_matters_match must be non-null
		{
			if (case_matters_match[0].search(/^\{case_matters=(true|false)\}$/) != -1)
			{
				// case_matters is valid!
				// trim the {case_matters=xx} and check case matters.
				str = str.substring(case_matters_match[0].length, str.length);
				order_match = FindOrderMatters(str);
				
				if (order_match != null && 
					(order_match[0].search(/^\{order_matters=(true|false)\}$/) == -1))
				{
					return false; // order matters failed validation
				}
					
				if (order_match != null)
				{
					// trim the {order_matters=xxxx}
					str = str.substring(order_match[0].length, str.length);
				}
			}
			else 
			{
				return false; // case_matters failed validation
			}
		}
		
		// now we are sure the string has valid delimiters for case_matters and order_matters. 
		// this function should continue split the string with [,] and check 
		// if each segment is a valid localized string.
		
		var substrings = str.split('[,]');
		for (var si in substrings)
		{
			if (!IsLocalizedStringType(substrings[si]))
			{
				// failed validation for some sub string.
				return false;
			}
		}
		
		return true;
	}
	
	// for type long-fill-in
	// The format is {case_matters=xx}<localized string>
	function IsLongFillIn(str)
	{
		// Deal with common cases to avoid reg-expression check everytime
		// If a string has delimiters, it must start with "{", longer than 4 characters "{x=}"
		// and must have character = and '}'
		if (str.length < 4 || str.substring(0,1) != '{' || str.search(/\=.*\}/) == -1)
			return true; // no delimiters found. It must be valid.

		// Find case_matters, if any
		var case_matters_match = FindCaseMatters(str);
		
		if (case_matters_match == null)
		{
			return IsLocalizedStringType(str);
		}
		else 
		{
			if (case_matters_match[0].search(/^\{case_matters=(true|false)\}$/) != -1)
			{
				// case_matters is valid!
				// trim the {case_matters=xx} and check case matters.
				str = str.substring(case_matters_match[0].length, str.length);
				return IsLocalizedStringType(str);
			}
			else 
			{
				return false; // case_matters failed validation
			}
		}
	}  
	
	// <min>[:]<max>
	function IsMinMaxNumeric(str)
	{
		var index = str.search(/\[:]/);
		if (index == -1)
			return false;
		var min = str.substring(0, index);
		var max = str.substring(index + '[:]'.length, str.length);
		if(min.length > 0 && max.length > 0 && IsReal(min) && IsReal(max))
		{
		    return Number(max) >= Number(min);
		}
		return ( (min.length == 0 || IsReal(min)) && (max.length == 0 || IsReal(max) ));
	}
	
	// only accept -1, 0, and 1. 
	// used by cmi.learner_preference.audio_captioning
	function IsAudioCaptioning(str)
	{
		return (str == '-1' || str == '0' || str == '1');
	}
	
	// used by cmi.objectives.n.success_status
	function IsSuccessStatus(str)
	{
		return (str == 'passed' || str == 'failed' || str == 'unknown');
	}
	// used by cmi.time_limit_action
	function IsTimeLimitAction(str)
	{
		return (str == 'exit,message' || str == 'continue,message' || str == 'exit,no message' || str == 'continue,no message');
	}
	
	// used by adl.nav.request
	function IsAdlNavRequest(str)
	{
		// It must be one of continue, previous, choice, exit, exitAll, abandon, abandonAll, and _none_
		// If it's 'choice', there must be a delimiter {target=<STRING>} before it.
		var reg = /^(continue|previous|\{target=.*\}choice|exit|exitAll|abandon|abandonAll|_none_)$/;
		return reg.test(str);
	}
	
	function IsLongIdentifier(str)
	{
	    if(str == "urn:") // special case this as it is an invalid urn:
	    {
	        return false;
	    }
	    return IsShortIdentifier(str);
	}
	
	function IsLikert(str)
	{
	    return IsShortIdentifier(str);
	}
	
	// short_identifier type
	function IsShortIdentifier(str)
	{
	    // Valid characters are : A-Z, a-z, 0-9, ?:;@&=+-.$,/_!~*'() and %HexHex
	    // The string cannot be empty. 
        var reg = /^([A-Za-z0-9\?:;@&\=\+\-\.\$,\/_!~\*'\(\)%])+$/;
	    if (! str.match(reg)) 
	        return false;
	        
	    // now make sure % is followed with HexHex
	    var hexReg = /^[a-f0-9]{2}$/i;
	    while (true)
	    {
    	    var index = str.indexOf('%');
    	    if (index < 0) break;
    	    str = str.substring(index + 1, str.length);
	        if (str.length < 2) return false;
	        if (!str.substring(0, 2).match(hexReg)) return false;
	    }
	    return true;
	}
	
	// used by adl.nav.request_valid.
	function IsTrueFalseUnknown(str)
	{
		// It must be one of true, false and unknown
		return str == 'true' || str == 'false' || str == 'unknown';
	}

	// used by multiple-choice type.
	function IsChoice(str)
	{
		// The value is separated with [,]
		// It could be empty, or no [,].
		// If there are delimiters [,], the separated sub strings must not be identical.
		if (str == "") return true;
		
		var substrs = str.split('[,]');
		for (var i = 0; i < substrs.length; i++)
		{
    		// each of the string must be a short_identifier
    		if (! IsShortIdentifier(substrs[i]))
    		{
    		    return false;
    		}
    		
		    for (var j = i+1; j < substrs.length; j++)
		    {
		        if (substrs[i] == substrs[j])
		            return false;
		    }
		}
		return true;
	}
	
	function IsMatching_LearnerResponse(str)
	{
	    // Learner response allows empty string.
	    if (str == "") 
	        return true;
	
	    var substrs = str.split("[,]");
	    for (var i = 0; i < substrs.length; i++)
	    {
	        var pair = substrs[i];
	        var idx = pair.indexOf("[.]");
	        if (idx < 0) 
	            return false; // This pair doesn't have [.] as separator
	        if (pair.indexOf("[.]", idx + 1) >= 0) 
	            return false; // This pair has more than one [.] separator.
	        if (!IsShortIdentifier(pair.substring(0, idx)) || 
	            !IsShortIdentifier(pair.substring(idx + 3, pair.length)) )
	            return false;
	    }
	    return true;
	}
	
	// used by matching type for cmi.interactions.n.correct_responses.n.pattern.
	// This pattern doesn't allow empty string.
	function IsMatching_CorrectPattern(str)
	{
	    // the format should be shortid[.]shortid[,]shortid[.]shortid
	    if (str == "") 
	        return false;
	    
	    return IsMatching_LearnerResponse(str);
	}
	
    // used by matching type for cmi.interactions.n.correct_responses.n.pattern.
	function IsSequencing(str)
	{
	    // <short_identifier_type>[,]<short_identifier_type>
		if (str == "") return false;
		
	    var substrs = str.split("[,]");
	    for (var i = 0; i < substrs.length; i++)
	    {
	        if (!IsShortIdentifier(substrs[i])) 
                return false;
        }
        return true;
	}

    // The format is <step_name>[.]<step_answer>[,]<step_name>[.]<step_answer>
    // Empty string is not allowed.
    function IsPerformance_LearnerResponse(str)
    {
	    if (str == "") 
	        return false;
	        
	    var substrs = str.split("[,]");
	    for (var i = 0; i < substrs.length; i++)
	    {
	        var pair = substrs[i];
	        var idx = pair.indexOf("[.]");
	        if (idx < 0) 
	            return false; // This pair doesn't have [.] as separator
	        if (pair.indexOf("[.]", idx + 1) >= 0) 
	            return false; // This pair has more than one [.] separator.

	        var stepName = pair.substring(0, idx);
	        var stepAnswer = pair.substring(idx + 3, pair.length);
	            
	        if (stepName == "")
	        {
    	        // The step name and the step answer cannot be empty at the same time. 
	            if (stepAnswer == "") 
	                return false;
	        }
	        else 
	        {
	            // In case the step name is not empty, it must be a short_identifier type
    	        if (!IsShortIdentifier(stepName)) 
                    return false;
            }
        }
	    return true;
	}

    // used by matching type for cmi.interactions.n.correct_responses.n.pattern.
	function IsPerformance_CorrectPattern(str)
	{
	    // the format should be {order_matters}<step_name>[.]<step_answer>[,]<step_name>[.]<step_answer>
	    if (str.indexOf("{order_matters=") == 0 && str.indexOf("}") > 0)
	    {
	        // it starts with order_matters delimiter. 
	        if (str.indexOf("{order_matters=true}") != 0 && str.indexOf("{order_matters=false}") != 0)
	        {
	            return false; // the delimiter is invalid.
	        }
	        // now remove the delimiter.
	        str = str.substring(str.indexOf("}") + 1, str.length);
	    }
	    
	    // now check if it is <step_name>[.]<step_answer>[,]<step_name>[.]<step_answer>
	    return IsPerformance_LearnerResponse(str);
	}
}