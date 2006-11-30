/* Copyright (c) Microsoft Corporation. All rights reserved. */
/* MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. */

// This function returns the success status of the passed in objective ID.
function checkObjectiveStatus(OBJ)
{
	var objCount = retrieveDataValue("cmi.objectives._count");
	if (objCount > 0)
	{
		for (n=0;n<objCount;n++)
		{
			id = retrieveDataValue("cmi.objectives."+n+".id");
			if (id==OBJ) {
				return retrieveDataValue("cmi.objectives."+n+".success_status");
			}
		}
	}
	else return null;
}

// This function returns the correct answer HTML
function correctAns() {
	return '<p align="center"><font color="#008800"><b>Correct!</b></font></p>'
}

// This funciton returns the incorrect answer HTML
function incorrectAns() {
	return '<p align="center"><font color="#FF0000"><b>Incorrect</b></font></p>'
}
