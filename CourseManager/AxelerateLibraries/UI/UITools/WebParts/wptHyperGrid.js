var _isIE = (document.all) ? true : false;

var KeyUP = 38;
var KeyLEFT = 37;
var KeyRIGHT = 39;
var KeyDOWN = 40;
var KeyTAB = 9;
var KeyENTER = 13;
var KeyF2 = 113;
var KeyESCAPE = 27;
var KeyDELETE = 46;
var KeyBACK = 8;

var _ActiveCell = null;
var _axGrid_CellInput = null;
var _EditingGridID = null;
var _LastColumnSelectedNum = null;
var _LastRowSelectedNum = null;

//Set document events, to manage grid edit in place management
document.onkeydown = OnDocumentKeyDown;
//document.onmousedown = OnDocumentMouseDown;
document.onmouseup = OnDocumentMouseUp;
function OnDocumentKeyDown(e)
{
    //I do it, for multi-browser support
    e = e || window.event;
    var target = e.target || e.srcElement;
    
    if(_axGrid_CellInput !== null && _ActiveCell !== null)
    {
        if(e.keyCode == KeyESCAPE) 
        {
            axGrid_EndEditCell(true);
        }
        else 
        { 
            if(e.keyCode == KeyENTER && target == _axGrid_CellInput) 
            {
                if(_axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "TEXT") {
                    axGrid_EndEditCell(false);
                    e.cancelBubble = true;
                    e.returnValue = false;
                    return false;
                }
            }
            else 
            { 
                if(target !== _axGrid_CellInput)
                {
                    axGrid_EndEditCell(false);
                }
            }
        }
    }
    
    //if control, alt are used for multiselect of cells. This is not supported yet.
    //if exist more that one grid on page, only one can has the focus, and only one cell can be the active one.
    //if not cell is active, we can't enter on edit mode or can navigate on the grid by keyboard because that mean that we don't have a grid with focus on it.
    if (e.ctrlKey || e.altKey || _ActiveCell == null)
        return;   
    
}

// this avoids wiring up of potentially thousands of cells
function OnDocumentMouseDown(e)
{
    e = e || window.event;
    var target = e.target || e.srcElement;
	
	if (_LastColumnSelectedNum !== null)
	{
	    var headertarget = axGrid_GetContainerCellHeader(target);
	    if (headertarget !== null && headertarget.id == (_EditingGridID + "_header_" + _LastColumnSelectedNum.toString()))
	    {
	        return;
	    }
	    else {
	        axGrid_DeselectColumn(_LastColumnSelectedNum, _EditingGridID);
	    }
	}
	if (_LastRowSelectedNum !== null)
	{
	    var headertarget = axGrid_GetContainerCellHeader(target);
	    if (headertarget !== null && headertarget.id == (_EditingGridID + "_headerrow_" + _LastRowSelectedNum.toString()))
	    {
	        return;
	    }
	    else {
	        axGrid_DeselectRow(_LastRowSelectedNum, _EditingGridID);
	    }
	}
	else {
	    if(_EditingGridID !== null)  {
	        if (_ActiveCell != null && (_axGrid_CellInput !== null))
            {
               var cellClicked = axGrid_GetContainerCell(target);
               var IsTargedInsideOftheActiveEditor = false;
               var oWalk = target;
               while(oWalk !== null && oWalk.getAttribute !== undefined && !IsTargedInsideOftheActiveEditor && oWalk !== _axGrid_CellInput.parentNode)
               {
                  if (oWalk == _axGrid_CellInput || (oWalk.attributes !== null && oWalk.getAttribute("stopediting") !== null && oWalk.getAttribute("stopediting").toUpperCase() == "FALSE" ))
                  {
                    IsTargedInsideOftheActiveEditor = true;
                  }
                  else {
                    oWalk = oWalk.parentNode;
                  }
               }
               if(_ActiveCell !== cellClicked && !IsTargedInsideOftheActiveEditor) 
               {
                   axGrid_EndEditCell(false);
               }
            }
        }
    }
    //if (e.cancelBubble == true) return; firefox don't respect the cancelBubble
	//alert(e.cancelBubble);
    
    
}
function OnDocumentMouseUp()
{
    // reenable text selection
    //document.onselectstart = null;
}




function axGrid_OnKeyPressed(e, TableID)
{
 //alert(event);
}

function axGrid_GetContainerCell(srcElement)
{
 var cell = null;
 var Elm = srcElement;
 
 if (Elm == _axGrid_CellInput)
 {
    cell = document.getElementById(Elm.getAttribute("celltoedit"));   
 }
 else {
     while(cell == null && Elm !== null && Elm !== undefined) 
     {
     
        if(Elm != null && Elm.nodeName !== undefined && Elm.nodeName.toUpperCase() == "TD" && Elm.id.match("_cell_"))
        {
            cell = Elm;
        }
        else {
            Elm = Elm.parentNode;
        }
     }
 }
 return cell;
}
function axGrid_GetContainerCellHeader(srcElement)
{
 var cell = null;
 var Elm = srcElement;
 
 while(cell == null && Elm !== null && Elm !== undefined) 
 {
 
    if(Elm != null && Elm.nodeName != undefined && Elm.nodeName.toUpperCase() == "TH" && (Elm.id.match("_header_") || Elm.id.match("_headerrow_")) )
    {
        cell = Elm;
    }
    else {
        Elm = Elm.parentNode;
    }
 }
 
 return cell;
}

function axGrid_GetNextCell(ActualCell, TableID)
{
   var nextCell = null;
   var cellIDParts = ActualCell.id.substring(TableID.length).split("_");
   var tableid = TableID;
   var elemType = cellIDParts[1]; //most be "cell"
   var rownumber = parseInt(cellIDParts[2],10);
   var cellnumber = parseInt(cellIDParts[3],10);
   var tmpNextCell = null;
   var NextCellFound = false;
   var actualrow = ActualCell.parentNode;
   var hndLocked = null;
   tmpNextCell = ActualCell;
  
  while (!NextCellFound) 
  {
      tmpNextCell = tmpNextCell.nextSibling;
      while(tmpNextCell !== null && getStyle(tmpNextCell,"display").toUpperCase() == "NONE") 
      {
          tmpNextCell = tmpNextCell.nextSibling;
      } 
      if (tmpNextCell !== null) 
      {
          //we need check if this cell not contains a non fix rows.
          cellIDParts = ActualCell.id.substring(TableID.length).split("_");
          if (cellIDParts.length == 4) 
          {
            hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
          }
          if (tmpNextCell.id.match(tableid + "_cell_" + rownumber.toString()) != null  && tmpNextCell.getAttribute("editable") !== null && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE" && hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if this is the case, cell.nextSibling is the next cell
          { 
            nextCell = tmpNextCell;
          }
          else {
            //must be a cell for non fix row
            if( tmpNextCell.id.match(tableid + "_cell_") == null) 
            {
                if (tmpNextCell.firstChild !== null && tmpNextCell.firstChild.nodeName.toUpperCase() == "DIV") {
                    if (tmpNextCell.firstChild.firstChild.nodeName.toUpperCase() == "TABLE") {
                        var nftb = tmpNextCell.firstChild.firstChild;
                        var nfr = nftb.rows[0];
                       
                        tmpNextCell = nfr.firstChild; //and then next cell can be the first cell on the new actual row.
                        if( tmpNextCell.id.match(tableid + "_cell_") !== null && getStyle(tmpNextCell,"display").toUpperCase() !== "NONE" && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE")
                        { 
                            cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
                            if (cellIDParts.length == 4) 
                            {
                                hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
                            }
                            if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
                            { 
                                nextCell = tmpNextCell;
                                NextCellFound = true;
                            }
                            
                        }
                    }
                }
                
            }
            
          }
      }
      if(nextCell == null && (tmpNextCell == null || tmpNextCell.nextSibling == null)) //if not exist a next cell on this row, i can try with the next row. i need check if this row is a non fixed, because if this is the case i need go to the next row of the grand parent table.
      {
          if (actualrow.id == (tableid + "_nfrow_" + rownumber.toString() ) ) //check if is a non fix row
          {
             actualrow = actualrow.parentNode.parentNode.parentNode.parentNode.parentNode; //actualrow.TBody.Table.Div(clipping).cell.row
             //parentNode of this actual row is the TableBody, the parentNode of the TableBody is a Table, the parent of this table is a div used for clipping the non fixed row, 
             //the parent of the clipping div is a cell with a colspan, and finally the parent of that cell is the fixed row of the grid table.
          }
          if(actualrow.nextSibling !== null && actualrow.nextSibling.id !== (tableid + "_row_" + (rownumber + 1).toString() )) //check if the next row not is child band hidden row.
          {
            actualrow = actualrow.nextSibling;
          }
          if (actualrow.nextSibling !== null) //check if we have other row
          {
              
              actualrow = actualrow.nextSibling; //the new actual row is the next row
              var rowidparts = actualrow.id.substring(TableID.length).split("_");
              rownumber = parseInt(rowidparts[2],10); // tableid_row_rownumber
              cellnumber = -1; //put to -1 indicating that no cell is selected yet.
              tmpNextCell = actualrow.firstChild; //and then next cell can be the first cell on the new actual row.
              if( tmpNextCell.id.match(tableid + "_cell_") !== null && getStyle(tmpNextCell,"display").toUpperCase() !== "NONE" && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE")
              { 
                cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
                if (cellIDParts.length == 4) 
                {
                    hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
                }
                if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
                {
                    nextCell = tmpNextCell;
                    NextCellFound = true;
                }
              }
              else if( tmpNextCell.id.match(tableid + "_cell_") == null) //in case that cell is a non fixed column container
              {
                if (tmpNextCell.firstChild.nodeName.toUpperCase() == "DIV")
                {
                    if (tmpNextCell.firstChild.firstChild.nodeName.toUpperCase() == "TABLE") 
                    {
                        var nftb = tmpNextCell.firstChild.firstChild;
                        var nfr = nftb.rows[0];
                        tmpNextCell = nfr.firstChild;
                        //check if tmpNextCell if the previews cell i am searching for.
                        if( tmpNextCell.id.match(tableid + "_cell_") !== null && getStyle(tmpNextCell,"display").toUpperCase() !== "NONE" && tmpNextCell.getAttribute("editable") !== null &&  tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE") 
                        {
                            cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
                            if (cellIDParts.length == 4) 
                            {
                                hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
                            }
                            if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
                            {
                                nextCell = tmpNextCell;
                                NextCellFound = true;
                            }
                        }
                    }
                }
              }
          }
          else {
              //no has next row, we can't find a next row we return null
              return null;
          }
          
      }
      else {
        if (nextCell !== null) {
            NextCellFound = true;
        }
      }
  }
  return nextCell;
   
}

function axGrid_GetPreviewsCell(ActualCell, TableID)
{
   var nextCell = null;
   var cellIDParts = ActualCell.id.substring(TableID.length).split("_");
   var tableid = TableID;
   var elemType = cellIDParts[1]; //most be "cell"
   var rownumber = parseInt(cellIDParts[2],10);
   var cellnumber = parseInt(cellIDParts[3],10);
   var tmpNextCell = null;
   var NextCellFound = false;
   var actualrow = ActualCell.parentNode;
   var hndLocked = null;
   
   tmpNextCell = ActualCell;
  
  while (!NextCellFound) 
  {
      tmpNextCell = tmpNextCell.previousSibling;
      while(tmpNextCell !== null && getStyle(tmpNextCell,"display").toUpperCase() == "NONE") 
      {
          tmpNextCell = tmpNextCell.previousSibling;
      } 
      if (tmpNextCell !== null) 
      {
          //we need check if this cell not contains a non fix rows.
          cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
          if (cellIDParts.length == 4) 
          {
            hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
          }
          if (tmpNextCell.id.match(tableid + "_cell_" + rownumber.toString()) != null  && tmpNextCell.getAttribute("editable") !== null && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE" && (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE")) //if this is the case, cell.nextSibling is the next cell
          { 
            nextCell = tmpNextCell;
          }
          else {
            //must be a cell for non fix row
            if( tmpNextCell.id.match(tableid + "_cell_") == null) 
            {
                if (tmpNextCell.firstChild.nodeName.toUpperCase() == "DIV") {
                    if (tmpNextCell.firstChild.firstChild.nodeName.toUpperCase() == "TABLE") {
                        var nftb = tmpNextCell.firstChild.firstChild;
                        var nfr = nftb.rows[0];
                        var nfcell = nfr.lastChild;
                        while(nfcell !== null && nextCell == null) 
                        {
                            if(getStyle(nfcell,"display").toUpperCase() !== "NONE") 
                            {
                                nextCell = nfcell;
                            }
                            else {
                                nfcell = nfcell.previousSibling;
                            }
                        }
                    }
                }
            }
            
          }
      }
      if(nextCell == null && (tmpNextCell == null || tmpNextCell.previousSibling == null)) //if not exist a next cell on this row, i can try with the next row. i need check if this row is a non fixed, because if this is the case i need go to the next row of the grand parent table.
      {
          if (actualrow.id == (tableid + "_nfrow_" + rownumber.toString() ) ) //check if is a non fix row
          {
             actualrow = actualrow.parentNode.parentNode.parentNode.parentNode.parentNode; //actualrow.TBody.Table.Div(clipping).cell.row
             //parentNode of this actual row is the TableBody, the parentNode of the TableBody is a Table, the parent of this table is a div used for clipping the non fixed row, 
             //the parent of the clipping div is a cell with a colspan, and finally the parent of that cell is the fixed row of the grid table.
          }
          if(actualrow.previousSibling !== null && actualrow.previousSibling.id.match(tableid + "_row_") == null) //check if the previews row not is child band hidden row.
          {
            actualrow = actualrow.previousSibling;
          }
          if (actualrow.previousSibling !== null) //check if we have other row
          {
              
              actualrow = actualrow.previousSibling; //the new actual row is the previews row
              var rowidparts = actualrow.id.substring(TableID.length).split("_");
              rownumber = parseInt(rowidparts[2],10); // tableid_row_rownumber
              cellnumber = -1; //put to -1 indicating that no cell is selected yet.
              tmpNextCell = actualrow.lastChild; //and then previews cell can be the first cell on the new actual row
              if( tmpNextCell.id.match(tableid + "_cell_") !== null && getStyle(tmpNextCell,"display").toUpperCase() !== "NONE" && tmpNextCell.getAttribute("editable") !== null && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE") 
              {
                cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
                if (cellIDParts.length == 4) 
                {
                    hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
                }
                if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
                {
                    nextCell = tmpNextCell;
                    NextCellFound = true;
                }
              }
              else if( tmpNextCell.id.match(tableid + "_cell_") == null) //in case that cell is a non fixed column container
              {
                if (tmpNextCell.firstChild !== null && tmpNextCell.firstChild.nodeName.toUpperCase() == "DIV")
                {
                    if (tmpNextCell.firstChild.firstChild.nodeName.toUpperCase() == "TABLE") 
                    {
                        var nftb = tmpNextCell.firstChild.firstChild;
                        var nfr = nftb.rows[0];
                        tmpNextCell = nfr.lastChild;
                        //check if tmpNextCell if the previews cell i am searching for.
                        if( tmpNextCell.id.match(tableid + "_cell_") !== null && getStyle(tmpNextCell,"display").toUpperCase() !== "NONE" && tmpNextCell.getAttribute("editable") !== null && tmpNextCell.getAttribute("editable").toUpperCase() == "TRUE") 
                        {
                            cellIDParts = tmpNextCell.id.substring(TableID.length).split("_");
                            if (cellIDParts.length == 4) 
                            {
                                hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
                            }
                            if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
                            {
                                nextCell = tmpNextCell;
                                NextCellFound = true;
                            }
                        }
                    }
                }
                
              }
          }
          else {
              //no has next row, we can't find a next row we return null
              return null;
          }
          
      }
      else {
        if (nextCell !== null) 
        {
            NextCellFound = true;
        }
      }
  }
  return nextCell;
   
}

function axGrid_OnKeyDown(e, TableID)
{
 e = e || window.event;
 var target = e.target || e.srcElement;
 var cell = axGrid_GetContainerCell(target);
 
 if(_ActiveCell == null) //if not has a cell selected, i can't navigate on the grid.
   return;
 
 if ((e.shiftKey == false && e.keyCode == KeyTAB) || (e.keyCode == KeyRIGHT && target !== _axGrid_CellInput) && cell !== null) 
 {
   e.cancelBubble = true;
   var nextcell = axGrid_GetNextCell(cell, TableID);
    if(nextcell != null) {
        //nextcell.scrollIntoView();
        //axGrid_cellClick(event, nextcell, sender);
         axGrid_EnterEditCell(nextcell,  TableID);
        return false; 
    }
 }
 else if ((e.shiftKey == true && e.keyCode == KeyTAB) || (e.keyCode == KeyLEFT && target !== _axGrid_CellInput) && cell !== null) 
 {
   e.cancelBubble = true;
   var previewscell = axGrid_GetPreviewsCell(cell, TableID);
    if(previewscell != null) {
        //nextcell.scrollIntoView();
        //axGrid_cellClick(event, nextcell, sender);
         axGrid_EnterEditCell(previewscell,  TableID);
        return false; 
    }
 
 }
}

function axGrid_DeselectColumn(cellnumber, tableid)
{
    var rownumber = 0;
    var maingrid = document.getElementById(tableid);
    if (maingrid !== null && maingrid.getAttribute("selectedrowcssclass") !== null) 
    {
        if (cellnumber !== null)
        {
            var lastcolumn = document.getElementById(_EditingGridID + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
            while (lastcolumn !== null)
            {
                repeatcol = 0;
                axGrid_RemoveClass(lastcolumn, maingrid.getAttribute("selectedrowcssclass"));
                lastcolumn = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString() + "_repeatable_" + repeatcol.toString());
                while (lastcolumn !== null)
                {
                    axGrid_RemoveClass(lastcolumn, maingrid.getAttribute("selectedrowcssclass"));
                    repeatcol++;
                    lastcolumn = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString() + "_repeatable_" + repeatcol.toString());
                }
                rownumber++;
                lastcolumn = document.getElementById(_EditingGridID + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());                    
            }
        }
    } 
    _LastColumnSelectedNum = null;
    _EditingGridID = null;
}

function axGrid_DeselectRow(rownumber, tableid)
{
    var cellnumber = 0;
    var maingrid = document.getElementById(tableid);
    if (maingrid !== null && maingrid.getAttribute("selectedrowcssclass") !== null) 
    {
        if (cellnumber !== null)
        {
            var lastcolumn = document.getElementById(_EditingGridID + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
            while (lastcolumn !== null)
            {
                axGrid_RemoveClass(lastcolumn, maingrid.getAttribute("selectedrowcssclass"));
                cellnumber++;
                lastcolumn = document.getElementById(_EditingGridID + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());                    
            }
        }
    } 
    _LastRowSelectedNum = null;
    _EditingGridID = null;
}

function axGrid_SelectColumn(cellnumber, tableid)
{
    var maingrid = document.getElementById(tableid);    
    var rownumber = 0;
    var repeatcol = 0;
    var coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
    if (coltag == null)
    {
        coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString() + "_repeatable_" + repeatcol.toString());
        repeatcol++;
    }
    while(coltag !== null) 
    {
        if (maingrid !== null && maingrid.getAttribute("selectedrowcssclass") !== null) 
        {
            axGrid_AddClass(coltag, maingrid.getAttribute("selectedrowcssclass"));            
        }
        coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString() + "_repeatable_" + repeatcol.toString());
        repeatcol++;
        if (coltag == null)
        {
            rownumber++;
            coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
            repeatcol = 0;
        }
    }
    var hndSelectCell = document.getElementById(tableid + "_selectedcolumn");
    hndSelectCell.value = cellnumber;
    _EditingGridID = tableid;
    _LastColumnSelectedNum = cellnumber;
}

function axGrid_SelectRow(rownumber, tableid)
{
    var maingrid = document.getElementById(tableid);    
    var cellnumber = 0;
    var coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
    while(coltag !== null) 
    {
        if (maingrid !== null && maingrid.getAttribute("selectedrowcssclass") !== null) 
        {
//            if (_LastRowSelectedNum !== null)
//            {
//                var lastcolumn = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + _LastRowSelectedNum.toString());
//                if (lastcolumn !== null)
//                {
//                    axGrid_RemoveClass(lastcolumn, maingrid.getAttribute("selectedrowcssclass"));
//                }
//            }
            axGrid_AddClass(coltag, maingrid.getAttribute("selectedrowcssclass"));            
        }
      cellnumber++;
      coltag = document.getElementById(tableid + "_cell_" + rownumber.toString() + "_" + cellnumber.toString());
    }
    var hndSelectCell = document.getElementById(tableid + "_selectedrow");
    hndSelectCell.value = rownumber;
    _EditingGridID = tableid;
    _LastRowSelectedNum = rownumber;
}

function axGrid_cellClick(e, TableID) 
{
    e = e || window.event;
    var target = e.target || e.srcElement;
    
    e.returnValue = true;
    e.cancelBubble = false;
	//if(_EditingGridID !== null &&  _EditingGridID !== "" && _EditingGridID !== TableID) return true; //if the event not is for this grid exit edition.
	    
    var container = axGrid_GetContainerCellHeader(target); //try to get a cell for the clicked element, this event only arraise inside a table.
    if (container !== null)
    {
        if (container.nodeName == 'TH' && (container.id.match(TableID + "_header_") || container.id.match(TableID + "_headerrow_")))
        {   
          OnDocumentMouseDown(e);
          var cellIDParts = container.id.substring(TableID.length).split("_");
          var tableid = TableID;
          if (cellIDParts.length == 3 && cellIDParts[1] == "header") {
              var cellnumber = parseInt(cellIDParts[2],10);
              
              axGrid_SelectColumn(cellnumber, tableid)
              e.returnValue = false;
              e.cancelBubble = true;
              return false;
          }
          if (cellIDParts.length == 3 && cellIDParts[1] == "headerrow") {
              var cellnumber = parseInt(cellIDParts[2],10);
              
              axGrid_SelectRow(cellnumber, tableid)
              e.returnValue = false;
              e.cancelBubble = true;
              return false;
          }
        }
    }
    else
    {     
        container = axGrid_GetContainerCell(target);
        if (container !== null && container.nodeName == 'TD' && container.id.match(TableID + "_cell_")  && container.getAttribute("editable") !== null && container.getAttribute("editable").toUpperCase() == "TRUE")
        {
            OnDocumentMouseDown(e);
            var cellIDParts = container.id.substring(TableID.length).split("_");
            var hndLocked = null;
            if (cellIDParts.length == 4) 
            {
                hndLocked = document.getElementById(TableID + "_lock_" + cellIDParts[3]);
            }
            if (hndLocked !== null && hndLocked.value.toUpperCase() !== "TRUE") //if the column is not locked
            {
                
                axGrid_EnterEditCell(container,  TableID);
                e.returnValue = false;
                e.cancelBubble = true;
                return false;
            }
            
            // prevent text selection (needed only if use select cell without editing.
            //document.onselectstart = function() { return false; }; 
            
        }
    }
   
}

function axGrid_LockedUnlockColumn(e, sender, TableID, ColumnIndex, LockImgUrl, UnlockImgUrl)
{
    var hndLocked = null;
    hndLocked = document.getElementById(TableID + "_lock_" + ColumnIndex);
    OnDocumentKeyDown(e);
    
    if (hndLocked !== null)
    {
        if (hndLocked.value.toUpperCase() == "TRUE")
        {
            hndLocked.value = "FALSE";
            sender.src = UnlockImgUrl;
        }
        else {
            hndLocked.value = "TRUE";
            sender.src = LockImgUrl;
        }
    }
}

function axGrid_cellDobleClick(e,  tableID)
{

    
}

function axGrid_OnAutomaticScroll(e, TableID)
{
    e = e || window.event;
    var target = e.target || e.srcElement;
    var cell = axGrid_GetContainerCell(target);
    
 //   alert(target.id);
 //   alert(cell.id);
}

function axGrid_EnterEditCell(cell,  TableID) 
{
  var hndSelectCell = document.getElementById(TableID + "_selectedcell");
  //hndSelectCell.value = cell.id;
  
  var cellIDParts = cell.id.substring(TableID.length).split("_");
  var tableid = TableID;
  var elemType = cellIDParts[1]; //most be "cell"
  var rownumber = parseInt(cellIDParts[2],10);
  var cellnumber = parseInt(cellIDParts[3],10);
   
  
  hndSelectCell.value = rownumber + ":" + cellnumber;
   
  //cell.focus();
  if ( _axGrid_CellInput != null && _ActiveCell !== null) 
    {
        //must update.
        
        axGrid_EndEditCell(false);
    }
  _EditingGridID = TableID;
  _axGrid_CellInput = document.getElementById(_EditingGridID + "_input_" + cellnumber.toString());
  
   //alert(_axGrid_CellInput);
  
  
  if (_axGrid_CellInput !== null) {
        var _HiddenValue = document.getElementById(cell.id + "_value");
        
        _ActiveCell = cell;
        _axGrid_CellInput.setAttribute("celltoedit", cell.id);
        var leftpos = 0;
        var toppos = 0;
        if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
        {
            leftpos = axGrid_getOffsetLeft(cell, _axGrid_CellInput.parentNode.offsetParent);
            toppos = axGrid_getOffsetTop(cell, _axGrid_CellInput.parentNode.offsetParent);
        }
        else 
        {
            leftpos = axGrid_getOffsetLeft(cell, _axGrid_CellInput.offsetParent);
            toppos = axGrid_getOffsetTop(cell, _axGrid_CellInput.offsetParent);
        }
        
        if(_isIE)
        {
            leftpos = leftpos -1;
        } 
        if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
        {
            _axGrid_CellInput.parentNode.style.left = leftpos  + "px";
            _axGrid_CellInput.parentNode.style.top = toppos  + "px";
        }
        else {
            _axGrid_CellInput.style.left = leftpos  + "px";
            _axGrid_CellInput.style.top  = toppos + "px";
        }
        
        if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
        {
            if(!_isIE)
            {
                _axGrid_CellInput.parentNode.style.width  = (cell.offsetWidth - 5) + "px";
                _axGrid_CellInput.parentNode.style.height = (cell.offsetHeight - 6) + "px";
            }
            else {
               _axGrid_CellInput.parentNode.style.width  = (cell.offsetWidth ) + "px";
               _axGrid_CellInput.parentNode.style.height = (cell.offsetHeight) + "px";
            }
        }
        else {
            if(!_isIE)
            {
                _axGrid_CellInput.style.width = (cell.offsetWidth - 5) + "px";
                _axGrid_CellInput.style.height = (cell.offsetHeight - 6) + "px";
            }
            else {
               _axGrid_CellInput.style.width = (cell.offsetWidth ) + "px";
               _axGrid_CellInput.style.height = (cell.offsetHeight) + "px";
            }
        }
        
        _axGrid_CellInput.style.zIndex = "999999";
        if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
        {
            _axGrid_CellInput.parentNode.style.zIndex = "999999";
        }
        //_Input.type = "text";
        if (_HiddenValue !== null) 
        {
            if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
            {
                _axGrid_CellInput.checked = (_HiddenValue.value.toUpperCase() == "TRUE");
            }
            
            _axGrid_CellInput.value = _HiddenValue.value;
            _axGrid_CellInput.setAttribute("value", _HiddenValue.value);
            
        }
     
        _axGrid_CellInput.tabIndex = 0;
         //cell.appendChild(_Input);
         //alert("dia si");
        _axGrid_CellInput.style.display = "";
        if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
        {
            _axGrid_CellInput.parentNode.style.display = "";
        }
        //_Input.focus();
        //_Input.select();
        //alert("dia hizo todo");
        setTimeout('setFocusToInput()', 1);
        
    }
}

function setFocusToInput(id) 
{
   
    if (_axGrid_CellInput !== null && _axGrid_CellInput.style.display.toUpperCase() !== "NONE") {
        _axGrid_CellInput.focus();
        if (_axGrid_CellInput.select !== undefined)  
        {
            _axGrid_CellInput.select();
        }
    }
}

function axGrid_CommitActiveCell()
{
    if(_ActiveCell !== null && _axGrid_CellInput !== null) {
        axGrid_EndEditCell(false);
    }
}

function axGrid_EndEditCell(cancel)
{
    
    if(_ActiveCell !== null && _axGrid_CellInput !== null) {
         
         var cmd = null;
         var cellID = _ActiveCell.id;
         var cellIDParts = _ActiveCell.id.substring(_EditingGridID.length).split("_");
         var tableid = _EditingGridID;
         var elemType = cellIDParts[1]; //most be "cell"
         var rownumber = parseInt(cellIDParts[2],10);
         var cellnumber = parseInt(cellIDParts[3],10);
         
         var _HiddenValue = null;
         var _Label = null;
         var _MainTable = document.getElementById(_EditingGridID);
         
         _HiddenValue = document.getElementById(cellID + "_value");
         
         if(_axGrid_CellInput.getAttribute("serveronedit") !== null && _axGrid_CellInput.getAttribute("serveronedit") !== "" && _HiddenValue.value !== _axGrid_CellInput.value && !cancel && _ActiveCell.getAttribute("rdk") !== null &&  _ActiveCell.getAttribute("rdk")!= "")
         { 
            //replace parameters of callback to commit changes to server
            cmd = _axGrid_CellInput.getAttribute("serveronedit");
            cmd = cmd.replace(/[[]rdk[]]/i,_ActiveCell.getAttribute("rdk"));
            cmd = cmd.replace(/[[]cdk[]]/i,_ActiveCell.getAttribute("cdk"));
            cmd = cmd.replace(/[[]cellindex[]]/i,cellnumber.toString());
            cmd = cmd.replace(/[[]value[]]/i,_axGrid_CellInput.value.replace(/\\/g,"\\\\"));
         }
         var isCheckbox = false;
         if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
         {
           isCheckbox = true;
         }
         if (_HiddenValue !== null && (_HiddenValue.value !== _axGrid_CellInput.value || isCheckbox) && !cancel )
         {
            
            _Label = document.getElementById(cellID + "_lbl");
            
            
            _HiddenValue.value = _axGrid_CellInput.value;
            if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
            {
                if(_axGrid_CellInput.checked)
                {
                   _HiddenValue.value = "true";
                }
                else {
                    _HiddenValue.value = "false";
                }
            }
               
            
            if(_Label !== null) {
                if (_axGrid_CellInput.getAttribute("displayvalue") !== null)
                {
                    _Label.innerHTML = _axGrid_CellInput.getAttribute("displayvalue");
                }
                else {
                    if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
                    {
                        if(_axGrid_CellInput.checked)
                        {
                            _Label.innerHTML = "Yes";
                        }
                        else {
                            _Label.innerHTML = "No";
                        }
                    }
                    else
                    {
                        _Label.innerHTML = _HiddenValue.value;
                    }
                    
                }
            }
            
            if (_MainTable !== null && _MainTable.getAttribute("editedrowcss") !== null) 
            {
                axGrid_AddClass(_ActiveCell, _MainTable.getAttribute("editedrowcss"))
            }
            
         }
         _axGrid_CellInput.style.display = "none";
         if(_axGrid_CellInput.nodeName != undefined && _axGrid_CellInput.nodeName.toUpperCase() == "INPUT" && _axGrid_CellInput.type.toUpperCase() == "CHECKBOX")
         {
            _axGrid_CellInput.parentNode.style.display = "none";
         }
         _axGrid_CellInput.setAttribute("celltoedit", "");
         _ActiveCell = null;
         _axGrid_CellInput = null;
         _EditingGridID = null;
         
         if(cmd != null && !cancel) 
         {
            //commit changes to server
            eval(cmd);
         }
     }

}


function axGrid_AddClass(el, className)
{
	if (el.className == null)
		el.className = className;
	else if (!axGrid_ContainsClass(el, className))
		el.className += (el.className.length > 0 ? ' ' : '') + className;
}

function axGrid_RemoveClass(el, className)
{
	if (el.className == null)
		return;
		
	var r = new RegExp('\\s*\\b' + className + '\\b\\s*');

	// remove the class and trim leading and trailing spaces	
	el.className = el.className.replace(r, ' ').replace(/^\s*(.*?)\s*$/, '$1');
}

function axGrid_ContainsClass(el, className)
{
	var r = new RegExp('\\b' + className + '\\b');
	
	return r.test(el.className);
}

function getStyle(oElm, strCssRule){
var strValue = "";
if(document.defaultView && document.defaultView.getComputedStyle){
    strValue = document.defaultView.getComputedStyle(oElm, "").getPropertyValue(strCssRule);
}
else if(oElm.currentStyle){
    strCssRule = strCssRule.replace(/\-(\w)/g, function (strMatch, p1){
        return p1.toUpperCase();
    });
    strValue = oElm.currentStyle[strCssRule];
}
return strValue;
}

       
function axGrid_getOffsetTop(element, topparent)
{
// walk up the stack of elements collecting
// the offset top values
var offsetTop = new Number(0);
try
{

    offsetTop = axGrid_getInt(element.offsetTop);
    //alert(element.id + ' : ' + offsetTop);
    var op = element.offsetParent;
    while ( op && op !== topparent )
    {
       //alert(op);
        offsetTop += axGrid_getInt(op.offsetTop);
        //alert(op.id + ' : ' + op.offsetTop);
        op = op.offsetParent;
    }
}
catch (ex) {}
//alert(offsetTop);
return offsetTop;
}

function axGrid_getOffsetLeft(element, topparent)
{
// walk up the stack of elements collecting
// the offset left values
var offsetLeft = new Number(0);
try
{
    offsetLeft = axGrid_getInt(element.offsetLeft);
    var op = element.offsetParent;
    while ( op && op !== topparent) //op.style.position == ""
    {
        offsetLeft += axGrid_getInt(op.offsetLeft);
        op = op.offsetParent;
    }
}
catch (ex) {}
return offsetLeft;
}

function axGrid_getInt(value)
{
	var num;
	if ( value )
	{
		num = parseInt(value);
		if (isNaN(num))
		{
			num = new Number(0);
		}
	}
	else
	{
		num = new Number(0);
	}
	return num;
}

function OnChildBandCollapseExpand(e,childbandstatus, childbandID, imgCEButtonID, imgCollapseUrl, imgExpandUrl) 
{
        OnDocumentMouseDown(e);

        var elem = document.getElementById(childbandID);
        var imgCEButton = document.getElementById(imgCEButtonID);
        
        if (elem.style.display == 'none')
        {
            elem.style.display = '';
            imgCEButton.src = imgCollapseUrl;
            imgCEButton.alt = "Collapse";
        }
        else
        {
            elem.style.display = 'none';
            imgCEButton.src = imgExpandUrl;
            imgCEButton.alt = "Expand";
        }
        var status = document.getElementById(childbandstatus);
        status.value = elem.style.display;
};

function axGrid_SynchronizeScroll(e, sender, receivername)
{
            if (sender.scrollLeft !== 0)
            {
                var receiver = document.getElementById(receivername);
                if (receiver !== null)
                {
                receiver.scrollLeft = sender.scrollLeft;   
                sender.scrollLeft = 0;
            
//            e = e || window.event;
//            
//            e.cancelBubble = true;
//            e.returnValue = false;
//            return false;  
                }
            }
}

function OnGridScroll(sender, panelsIDsToMove)
{
    OnDocumentMouseDown(window.event);
    var panelsids = panelsIDsToMove.split(";");
    for(var ni=0; ni < panelsids.length; ni++) 
    {
        if (panelsids[ni] != "") {
            var panel = document.getElementById(panelsids[ni]);
            panel.style.left = (-1 * sender.scrollLeft) + "px";
        }
    }
}

function SetScrollHeight(scrollDiv, griddivid)
{
    var griddiv = document.getElementById(griddivid);
    //var scrollDiv = document.getElementById(griddivid);
    if(scrollDiv !== undefined && scrollDiv !== null && griddiv !== null) 
    {
        scrollDiv.style.height = scrollDiv.scrollHeight;
        
        if (parseInt(griddiv.style.height,10) >= griddiv.offsetHeight) 
        {
            scrollDiv.style["overflow-y"] = "hidden";
        }
        else {
           scrollDiv.style["overflow-y"] = "auto";
        }
    }
}


