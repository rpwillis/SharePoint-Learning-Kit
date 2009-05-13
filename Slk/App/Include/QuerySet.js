/* Copyright (c) Microsoft Corporation. All rights reserved. */

// QuerySet.js -- script for QuerySet.aspx

// globals
g_iSelectedQuery = null; // zero-base index of selected query

function HighlightQuery(iQuery)
{
	// highlight query <iQuery> (zero-based)...

	// update query label "<td>"
	var tdLabelStyle = document.getElementById("QueryLabelTD" + iQuery).style;
	tdLabelStyle.backgroundColor = "white";

	// update query label "<a>"
	var aLabelStyle = document.getElementById("QueryLabelA" + iQuery).style;
	aLabelStyle.color = "black";

	// update query count "<td>"
	var tdCountStyle = document.getElementById("QueryCountTD" + iQuery).style;
	tdCountStyle.backgroundColor = "white";

	//tdCountStyle.borderRight = "solid 1px white";
	tdCountStyle.borderRight = "";
	tdCountStyle.paddingRight = "15px";

	// update query count "<a>"
	var aCountStyle = document.getElementById("QueryCountA" + iQuery).style;
	aCountStyle.color = "black";

	// update borders
	document.getElementById("QueryLabelBorderTop" + iQuery).style.backgroundColor = "#E0E0E0";
	document.getElementById("QueryCountBorderTop" + iQuery).style.backgroundColor = "#E0E0E0";
	document.getElementById("QueryLeftBorder" + iQuery).style.backgroundColor = "#E0E0E0";
	document.getElementById("QueryLabelBorderBottom" + iQuery).style.backgroundColor = "#E0E0E0";
	document.getElementById("QueryCountBorderBottom" + iQuery).style.backgroundColor = "#E0E0E0";
}

function UnhighlightQuery(iQuery)
{
	// remove highlighting from query <iQuery> (zero-based)...

	// update query label "<td>"
	var tdLabelStyle = document.getElementById("QueryLabelTD" + iQuery).style;
	tdLabelStyle.backgroundColor = "";

	// update query label "<a>"
	var aLabelStyle = document.getElementById("QueryLabelA" + iQuery).style;
	aLabelStyle.color = "";

	// update query count "<td>"
	var tdCountStyle = document.getElementById("QueryCountTD" + iQuery).style;
	tdCountStyle.backgroundColor = "";
	tdCountStyle.borderRight = "solid 1px #E0E0E0";
	tdCountStyle.paddingRight = "14px";

	// update query count "<a>"
	var aCountStyle = document.getElementById("QueryCountA" + iQuery).style;
	aCountStyle.color = "";

	// update borders
	document.getElementById("QueryLabelBorderTop" + iQuery).style.backgroundColor = "";
	document.getElementById("QueryCountBorderTop" + iQuery).style.backgroundColor = "";
	document.getElementById("QueryLeftBorder" + iQuery).style.backgroundColor = "";
	document.getElementById("QueryLabelBorderBottom" + iQuery).style.backgroundColor = "";
	document.getElementById("QueryCountBorderBottom" + iQuery).style.backgroundColor = "";
}

function SetQueryCounts(aCounts)
{
	// this function is called from the hidden frame page (QuerySummary.aspx) when it has computed
	// the count of results of each query; this function updates the counts on this page, replacing
	// the initial hourglasses
	for (var iQuery = 0; iQuery < aCounts.length; iQuery++)
	{
		document.getElementById("QueryCountA" + iQuery).innerText = aCounts[iQuery];
		document.getElementById("QueryCountA" + iQuery).textContent  = aCounts[iQuery];
	}
}

function SetQueryCount(queryName, queryCount)
{	
	// this function is called from the Results frame page (QueryResults.aspx) when it has computed
	// the count of results of executed query; this function updates the counts 
	if (typeof(QueryNames) != "undefined")
	{
	    for (var iQuery = 0; iQuery < QueryNames.length; iQuery++)
	    {	 
	        if(QueryNames[iQuery] == queryName)
	        {
	            document.getElementById("QueryCountA" + iQuery).innerText = queryCount;
	            document.getElementById("QueryCountA" + iQuery).textContent = queryCount;
	            break;
	        }
	    }
	}		
}

