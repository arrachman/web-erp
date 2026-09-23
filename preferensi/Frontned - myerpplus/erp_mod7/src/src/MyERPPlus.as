import com.adobe.serialization.json.JSON;

import flash.display.DisplayObject;
import flash.events.ContextMenuEvent;
import flash.events.Event;
import flash.events.FocusEvent;
import flash.events.KeyboardEvent;
import flash.events.MouseEvent;
import flash.net.URLLoader;
import flash.net.URLRequest;
import flash.system.System;
import flash.ui.ContextMenu;
import flash.ui.ContextMenuItem;
import flash.ui.Keyboard;
import flash.utils.ByteArray;

import itemEditor.*;

import itemRenderer.*;

import mx.collections.ArrayCollection;
import mx.collections.ArrayList;
import mx.containers.TabNavigator;
import mx.controls.Alert;
import mx.controls.MenuBar;
import mx.controls.TextInput;
import mx.core.ClassFactory;
import mx.core.FlexGlobals;
import mx.core.FlexLoader;
import mx.core.IFactory;
import mx.core.UIComponent;
import mx.events.FlexEvent;
import mx.events.MenuEvent;
import mx.formatters.DateFormatter;
import mx.managers.CursorManager;
import mx.managers.FocusManager;
import mx.managers.PopUpManager;
import mx.messaging.AbstractConsumer;
import mx.rpc.soap.mxml.WebService;
import mx.utils.ObjectUtil;

import org.flexunit.runners.ParentRunner;

import spark.components.Button;
import spark.components.DataGrid;
import spark.components.Group;
import spark.components.Image;
import spark.components.Label;
import spark.components.gridClasses.GridColumn;
import spark.components.gridClasses.GridItemEditor;
import spark.components.supportClasses.ItemRenderer;
import spark.events.GridEvent;
import spark.events.GridItemEditorEvent;
import spark.events.GridSelectionEvent;
import spark.events.GridSortEvent;
import spark.events.IndexChangeEvent;
import spark.events.TextOperationEvent;
import spark.formatters.NumberFormatter;

public var ActiveRowIndex:int=0;
public var ActiveColumnIndex:int=0;

// pbut adalah popup button digudanakan sebagai pendukung ungsi F_BusyMode
public var pbut:Button = new Button;
public var ActiveGrid:Object;
//digunakan untuk kebutuhan pencarian data
public var target:String='';

public var vpIdTxt:Object, vpFocusEvent:Boolean = true, vpSetIn:Boolean = true, vpOnEnter:Boolean = false, vpVarCurrent:String = '', vpFocusEnter:Boolean = false;


//fungsi untuk mengosongkan .text pada textbox atau label
//example-- 
//F_KosongkanText([txtNama,txtKeterangan,LblTotalHarga]);

public function F_KosongkanText(ar:Array):void
{
	for (i=0;i<ar.length;i++)
	{
		ar[i].text = '';	
	}
}

//fungsi untuk memeberi nilai .text pada textbox atau label
//example-- 
//F_KosongkanText([txtNama,txtKeterangan,LblTotalHarga]);
public function F_SetNilaiText(ar:Array,nilai:String):void
{
	var i:int=0;
	for (i=0;i<ar.length;i++)
	{
		ar[i].text = nilai;	
	}
}

public function F_ComboBoxStandard(arCombo:Array):void {
	var i:int;
	var x:int=arCombo.length;
	for(i=0;i<x;i++) {
		if(arCombo[i].dataProvider == null)arCombo[i].dataProvider = new ArrayCollection;
		arCombo[i].addEventListener(FocusEvent.FOCUS_OUT,f_CombofocusOutHandler);
		arCombo[i].addEventListener(IndexChangeEvent.CHANGE,f_CombochangeHandler);
		arCombo[i].addEventListener(FlexEvent.UPDATE_COMPLETE,f_ComboupdateCompleteHandler);
		arCombo[i].addEventListener(KeyboardEvent.KEY_DOWN,f_CombokeyDownHandler);
		arCombo[i].selectedIndex=0;
		arCombo[i].labelField="l";	
	}
	
	function f_CombofocusOutHandler(event:FocusEvent):void {
		detect(event.currentTarget);
	}
	
	function f_CombochangeHandler(event:IndexChangeEvent):void {
		detect(event.currentTarget);
	}
	
	function f_ComboupdateCompleteHandler(event:FlexEvent):void {
		var tx:String="";
		var awal:Boolean=false;
		if(event.currentTarget.textInput.text.length==1) {
			awal=true;
			tx = event.currentTarget.textInput.text;
		}
		detect(event.currentTarget);
		if(awal==true) {
			event.currentTarget.textInput.text=tx;	
		}
	}
	
	function f_CombokeyDownHandler(event:KeyboardEvent):void {
		if(event.keyCode==Keyboard.ENTER) {
			focusMgr();
		}
	}
	
	function detect(ob:Object):void {
		var i:int;
		var x:int=ob.dataProvider.length;
		var stat:Boolean=false;
		for(i=0;i<x;i++) {
			if(ob.textInput.text==ob.dataProvider.getItemAt(i).l) {
				stat = true;
			}	
		}
		if(stat==false)
		{
			ob.textInput.text = "";
		}
		if(ob.selectedIndex<0)
		{
			ob.textInput.text = "";
		}
	}	
}

public function F_ComboBoxStandardFilter(arCombo:Array):void {
	var i:int;
	var x:int=arCombo.length;
	for(i=0;i<x;i++) {
		if(arCombo[i].dataProvider == null)arCombo[i].dataProvider = new ArrayCollection;
		arCombo[i].addEventListener(FocusEvent.FOCUS_OUT,f_CombofocusOutHandler);
		arCombo[i].addEventListener(IndexChangeEvent.CHANGE,f_CombochangeHandler);
		arCombo[i].addEventListener(FlexEvent.UPDATE_COMPLETE,f_ComboupdateCompleteHandler);
		arCombo[i].addEventListener(KeyboardEvent.KEY_DOWN,f_CombokeyDownHandler);
		arCombo[i].selectedIndex=1;
		arCombo[i].labelField="l";	
	}
	
	function f_CombofocusOutHandler(event:FocusEvent):void {
		detect(event.currentTarget);
	}
	
	function f_CombochangeHandler(event:IndexChangeEvent):void {
		detect(event.currentTarget);
	}
	
	function f_ComboupdateCompleteHandler(event:FlexEvent):void {
		var tx:String="";
		var awal:Boolean=false;
		if(event.currentTarget.textInput.text.length==1) {
			awal=true;
			tx = event.currentTarget.textInput.text;
		}
		detect(event.currentTarget);
		if(awal==true) {
			event.currentTarget.textInput.text=tx;	
		}
	}
	
	function f_CombokeyDownHandler(event:KeyboardEvent):void {
		if(event.keyCode==Keyboard.ENTER) {
			focusMgr();
		}
	}
	
	function detect(ob:Object):void {
		var i:int;
		var x:int=ob.dataProvider.length;
		var stat:Boolean=false;
		for(i=0;i<x;i++) {
			if(ob.textInput.text==ob.dataProvider.getItemAt(i).l) {
				stat = true;
			}	
		}
		if(stat==false)
		{
			ob.textInput.text = "";
		}
		if(ob.selectedIndex<0)
		{
			ob.textInput.text = "";
		}
	}	
}

public function F_PilihTabBar(ob:Object,ind:int):void
{
	ob.selectedIndex = ind;
	ob.validateNow();
}


//Digunakan untuk mendeteksi apakah kode keyboard adalah 0-9 atau a-z, bukan ctrl,alt,dll. .
public function F_NumToChar(num:int):String 
{
	if (num > 47 && num < 58) 
	{
		var strNums:String = "0123456789";
		return strNums.charAt(num - 48);
	} 
	else if (num > 64 && num < 91) 
	{
		var strCaps:String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		return strCaps.charAt(num - 65);
	} 
	else if (num > 96 && num < 123) 
	{
		var strLow:String = "abcdefghijklmnopqrstuvwxyz";
		return strLow.charAt(num - 97);
	} 
	else 
	{
		return '';
	}
}


//melakukan set pada textinput agar menjadi numberonly input
public function F_TextInputNumberOnlyMode(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = "0-9";
		arTextInput[i].addEventListener(FocusEvent.FOCUS_OUT,TextfocusOutHandler);
		arTextInput[i].addEventListener(spark.events.TextOperationEvent.CHANGE, TextChangeHandler);
		arTextInput[i].addEventListener(FocusEvent.FOCUS_IN,TextfocusInHandler);
		arTextInput[i].setStyle("textAlign","right");
		arTextInput[i].text = m.nfNumber.format('0');
		arTextInput[i].maxChars = 15;
		m[module].child.o[arTextInput[i].id] = 0;
	}
	
	F_SetNavigasiKeyboard(arTextInput);
	
	function TextfocusInHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m[idmod].child.o[event.currentTarget.id];
	}
	
	function TextChangeHandler(event:TextOperationEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = event.currentTarget.text;
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m.nfNumber.format(event.currentTarget.text);
	}
}

public function F_SetNavigasiKeyboard(ar:Array):void
{
	for(i=0;i<ar.length;i++)
		ar[i].addEventListener(KeyboardEvent.KEY_DOWN, Obj_keyDownHandler);	
	
	function Obj_keyDownHandler(event:KeyboardEvent):void
	{
		if((event.keyCode==Keyboard.DOWN)||(event.keyCode==Keyboard.ENTER))
			focusMgr();
		else if(event.keyCode==Keyboard.UP)
			focusMgr(true);
	}	
}
public function F_TranslateLabel(ar:Array):void
{
	for(var j:int=0;j<ar.length;j++){
		ar[j].text = l(ar[j].text);
		ar[j].validateSize(true);
	}
}

public function F_RequiredLabel(ar:Array, color:String = 'red'):void
{
	this.callLater(F_RequiredLabelLater, [ar, color]);
}

protected function F_RequiredLabelLater(ar:Array, color:String):void
{
	for(var j:int=0;j<ar.length;j++){
		m.l = new spark.components.Label;
		m.l.left = (int(ar[j].left)+int(ar[j].width)+3);
		m.l.y = ar[j].y;
		m.l.text = "*";
		m.l.setStyle('color', color);
		ar[j].parent.addElement(m.l);
	}
}
//melakukan set pada textinput agar menjadi numberonly input
public function F_TextInputNumberDesimalMode(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = m.nfNominal.decimalSeparator+"0-9";
		arTextInput[i].addEventListener(FocusEvent.FOCUS_IN, TextfocusInHandler);
		arTextInput[i].addEventListener(spark.events.TextOperationEvent.CHANGE, TextChangeHandler);
		arTextInput[i].addEventListener(FocusEvent.FOCUS_OUT, TextfocusOutHandler);
		arTextInput[i].setStyle("textAlign","right");
		arTextInput[i].text = m.nfNominal.format("0");
		arTextInput[i].maxChars = 15;
		m[module].child.o[arTextInput[i].id] = 0;
	}
	F_SetNavigasiKeyboard(arTextInput);
	function TextfocusInHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m[idmod].child.o[event.currentTarget.id];
	}
	
	function TextChangeHandler(event:TextOperationEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = event.currentTarget.text;
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = String(m[idmod].child.o[event.currentTarget.id]).split(m.nfNominal.decimalSeparator).join('.');
		event.currentTarget.text = m.nfNominal.format(event.currentTarget.text);
	}
}
//melakukan set pada textinput agar menjadi numberonly input
public function F_TextInputDiskonbertingkat(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for(i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = m.nfNominal.decimalSeparator+"+0-9";
		arTextInput[i].addEventListener(FocusEvent.FOCUS_IN, TextfocusInHandler);
		arTextInput[i].addEventListener(spark.events.TextOperationEvent.CHANGE, TextChangeHandler);
		arTextInput[i].addEventListener(FocusEvent.FOCUS_OUT, TextfocusOutHandler);
		arTextInput[i].setStyle("textAlign","right");
		arTextInput[i].text = m.nfNominal.format("0");
		arTextInput[i].maxChars = 15;
		m[module].child.o[arTextInput[i].id] = 0;
	}
	
	F_SetNavigasiKeyboard(arTextInput);
	
	function TextfocusInHandler(event:FocusEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = String(m[idmod].child.o[event.currentTarget.id]).split('.').join(m.nfNominal.decimalSeparator);
		event.currentTarget.text = m[idmod].child.o[event.currentTarget.id];
	}
	
	function TextChangeHandler(event:TextOperationEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = String(event.currentTarget.text).split(m.nfNominal.decimalSeparator).join('.');
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		m[idmod].child.o[event.currentTarget.id] = String(m[idmod].child.o[event.currentTarget.id]).split(m.nfNominal.decimalSeparator).join('.');
		m.temp = String(m[idmod].child.o[event.currentTarget.id]).split('+');
		if(m.temp.length > 1){
			m.s = m.nfNominal.format(m.temp[0]);
			for(i=1;i<m.temp.length;i++)
				m.s += "+"+m.nfNominal.format(m.temp[i]);
			event.currentTarget.text = m.s;
		}else event.currentTarget.text = m.nfNominal.format(m[idmod].child.o[event.currentTarget.id]);
	}
}

public function F_refreshTextNumber(txt:Array):void
{
	f_refreshIDmodule(txt[0]);
	for (i=0;i<txt.length;i++)
		if(m[module].child.o[txt[i].id] == undefined){
			m[module].child.o[txt[i].id] = 0;
			txt[i].text = m.nfNominal.format('0');
		}else
			txt[i].text = m.nfNominal.format(m[module].child.o[txt[i].id]);
}

//melakukan set pada textinput Currency Mode
public function F_TextInputCurrency(arTextInput:Array):void
{
	F_TextInputNumberDesimalMode(arTextInput);
}

//Menentukan Jumlah Maksimal Karakter pada TextIput
public function F_TextInputSetMaxChar(arTextInput:Array,MaxChar:int):void
{
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].maxChars = MaxChar;
	}
}


//Standarisasi Tanggal
public function F_DateFieldStandard(arNamaDateField:Array, modetxet:Boolean = false):void
{
	for(i=0;i<arNamaDateField.length;i++)
	{
		arNamaDateField[i].restrict = ". -/0-9";
		if(modetxet == false){
			arNamaDateField[i].formatString = m.formatDate.toUpperCase();
			arNamaDateField[i].editable = true;
		}
		arNamaDateField[i].addEventListener(FocusEvent.FOCUS_OUT,Df_focusOutHandler);
	}
	
	F_SetNavigasiKeyboard(arNamaDateField);
	
	function Df_focusOutHandler(event:FocusEvent):void
	{
		//variable pendukung
		var ob:Object = event.currentTarget;
		var dt:String = ob.text;
		var d:Array; var dTglDefault:Array;
		var tglDefault:String = FlexGlobals.topLevelApplication.DefaultTanggal;
		dTglDefault = tglDefault.split("/");
		
		//distandarkan format tanggalnya
		dt = dt.split(" ").join("/");
		dt = dt.split("-").join("/");
		d = dt.split('/');
		
		if(d.length == 1)
			switch(dt.length){
				case 1:f_valid("0"+dt, dTglDefault[1], dTglDefault[2]);break;
				case 2:f_valid(dt, dTglDefault[1], dTglDefault[2]);break;
				case 4:f_valid(dt.slice(0,2), dt.slice(2,4), dTglDefault[2]);break;
				case 6:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,6));break;
				case 8:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,8));break;
				default:ob.text = '';break;
			}
		else if(d.length == 2)
			f_valid(d[0], d[1], dTglDefault[2]);
		else if(d.length == 3)
			f_valid(d[0], d[1], d[2]);
		else ob.text = '';
		
		function f_valid(dd:int, mm:int, yyyy:int):void
		{
			var d:int, d1:String, d2:String;
			if(mm > 12)mm = 12;
			else if(mm < 1)mm = 1;
			if(yyyy == 0)yyyy = dTglDefault[2];
			else if(String(yyyy).length < 4)yyyy = dTglDefault[2];
			else if(yyyy > 2100) yyyy = 2100;
//			else if(yyyy < 1900) yyyy = 1900;
			d = FlexGlobals.topLevelApplication.F_getDayCount(yyyy, mm);
			if(dd > d) dd = d;
			else if(dd < 1) dd = 1;
			d1 = String(dd);
			d2 = String(mm);
			if(d1.length == 1)d1 = "0"+d1;
			if(d2.length == 1)d2 = "0"+d2;
			ob.text = d1+"/"+d2+"/"+yyyy;
		}		
	}
	
}

// ------------------------------------------------------ 26 April 2013 Fatkur --------------------------------------------

// --------- Panggil WS  ---------------

protected var wsValidasi:Boolean = true, wsParam:String = '';
public function F_callWS(Subject:String, _Operations:String, _paging:String = "", _userId:String = '0', _IsUpdate:Boolean = true, _data:String = ''):void
{
	if(wsValidasi == true){
		wsValidasi = false;
		this.subject = Subject;
		wsParam =  WebAccessKey + sptParam + _Operations + sptParam + _paging + sptParam + _userId + sptParam + int(_IsUpdate) + sptParam + _data;
		
		m.myWs.Ws(wsParam);
	}
}

// Ws Uplaod
public function F_wsUpload(Subject:String, paket:String, userid:String, filePaket:String, fileExtension:String, data:ByteArray, idtransaksi:int = 0, namafile:String = '', catatan:String = '', ukuranfile:String = '', tanggal:String = '', pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", idtransaksi2:String = '', fdefault:int = 0):void
{
	if(wsValidasi == true){
		wsValidasi = false;
		this.subject = Subject;
		wsParam = WebAccessKey + sptParam + paket + sptParam + pageNumber + sptSubParam + itemLimit + sptSubParam + strFilter + sptSubParam + strSort + sptSubParam + m.formatDate + sptSubParam + m.formatDate + " " + m.formatTime + sptParam + userid + "★0★" + filePaket + sptField + fileExtension + sptField + filePaket + sptField + idtransaksi + sptField + namafile + sptField + catatan + sptField + ukuranfile + sptField + tanggal + sptField + userid + sptField + DefaultTanggalforDB + sptField + idtransaksi2 + sptField + fdefault;
		m.wsUploadData = data;
		m.myWs.UploadFile(wsParam, data);
	}
}

// WS Simpan
public var vg_master:String = "";
public function F_wsSimpan(Subject:String, paket:String, userId:int, isUpdate:Boolean, dataUtama:String, dataDetail:String = '', pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = ''):void
{
	if (vg_master!="")dataDetail = vg_master;
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate + " " + m.formatTime;
	var s:String = dataUtama+sptSubParam+dataDetail;
	if(dataDetail == '')s = dataUtama;
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, String(userId), isUpdate, s);
	vg_master = "";
}

// WS Search
public function F_wsSearch(Subject:String, paket:String, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = ''):void
{
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu)
}

// WS GetDataById
public function F_wsGetDataById(Subject:String, paket:String, idTransaksi:String, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = ''):void
{
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, idTransaksi);
}

// WS Delete
public function F_wsDelete(Subject:String, paket:String, userId:int, kode:String, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = ''):void
{
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, String(userId), true, kode);
}

// WS Update Status
public function F_wsUpdateStatus(Subject:String, paket:String, userId:int, idTransaksi:int, nilaistatus:String, isDelete:Boolean, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = ''):void
{
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, String(userId), isDelete, idTransaksi+sptSubParam+nilaistatus);
}

// WS Get/Set Json
public function F_wsJson(Subject:String, paket:String, nameFile:String, content:String = ''):void
{
	subject = Subject;
	nameFile = nameFile + ".json";
	if(content != '')nameFile = nameFile+sptSubParam+content;
	if(paket == 'M0_SetFileLibrary')
		F_callWS(Subject, paket, '', '0', true, nameFile);
	else if(paket == 'M0_GetFileLibrary')
		loadJson.load(new URLRequest(m.url+nameFile));
	else if(paket == 'File')
		loadJson.load(new URLRequest(nameFile));
}

// -------------------------------------------------- Akhir fungsi-fungsi WS --------------------------------------------------
public var vKolomLength:int;
public function f_ArrDetail(data:String):ArrayCollection{
	var i:int, a:Array = data.split(sptRow), f:ArrayCollection = new ArrayCollection, s:String, b:Array, j:int;
	
	for(i=0;i<a.length;i++){
		b = a[i].split(sptField);
		f.addItem({no:(i+1)});
		for(j=0;j<b.length;j++)
			f[f.length-1]["f"+j] = b[j];
	}
	f.refresh();
	return f;
}

public function f_SplitData(judul:String, data:String):ArrayCollection{
	var i:int, j:int, f:ArrayCollection = new ArrayCollection, s:String, a:Array = judul.split(sptField), b:Array = data.split(sptRow), c:Array;
	for(i=0;i<b.length;i++){
		c = b[i].split(sptField);
		f.addItem({no:(i+1)});
		for(j=0;j<a.length;j++)
			f[f.length-1][a[j]] = c[j];
	}
	if(data == '')f.removeAll();
	f.refresh();
	return f;
}


// ------------------------------------------------------ 26 April 2013 Fatkur 
// --------------------------------- Pencarian ----------------------
protected function f_refreshIDmodule(obj:Object = null):void
{
	if(obj == null){
		if(idmod.split('DynamicLoader').length > 1)
//			idmod = f_cariIdmod(MySuperTab.getChildAt(MySuperTab.selectedIndex)['label'], m.ArrTab[MySuperTab.selectedIndex].loader);
			idmod = m.ArrTab[MySuperTab.selectedIndex].loader;
	}else{
		m.a = String(obj).split('.');
		if(m.a[0] == 'MyERPPlus')
			module = m.a[7];
		else if(m.a[0].split('winPopUp')['length'] > 1)
			module = m.a[5];
		else /*if(module != null){
			if(module.split('DynamicLoader').length == 0 && module.split('popUpLoader').length == 0)
				module = idmod;
		}else*/ module = idmod;
	}
}

public function F_txtpencarian(arr:Array):void{
	f_refreshIDmodule(arr[0]);
	ob = m[module].child;
	for(i=0;i<arr.length;i++)
		add(arr[i])
	 
	function add(txt:Object):void{
		txt.addEventListener(FocusEvent.FOCUS_IN, f_focusin);
		txt.addEventListener(FocusEvent.FOCUS_OUT, f_focusout);  
		txt.addEventListener(KeyboardEvent.KEY_DOWN, f_keydown);
		
		m.img = new Image();
		m.img.left = (txt.left+txt.width+3);
		m.img.top = (txt.top+3); 
		m.img.source = tbfind; 
		f_cariklik(m.img, txt);
		txt.parent.addElement(m.img); 
		
		function f_focusin(e:FocusEvent):void{
			vpIdTxt = txt;
			if(vpSetIn == true){
				vpVarCurrent = txt.text;			
				vpSetIn = false;
			}
		}
		function f_focusout(e:FocusEvent):void{
			if(vpOnEnter == false)vpFocusEvent = false;
			if(txt.text == '' && txt.id != null){
				if(ob.hasOwnProperty('f_kosongkanText'))ob.f_kosongkanText(txt.id);
				vpSetIn = true;
			}else if(txt.text != vpVarCurrent){
				if(vpFocusEnter == false){
					F_cari(txt.id);	
					vpSetIn = true;
				}
			}else if(txt.text == vpVarCurrent){
				vpSetIn = true;
			}
		}
		function f_keydown(e:KeyboardEvent):void{
			switch(e.keyCode){
				case Keyboard.F12:vpFocusEnter = true;vpFocusEvent=true;F_popUpCari(txt.id);break;
				case Keyboard.UP:focusMgr(true);break;
				case Keyboard.DOWN:focusMgr();break;
				case Keyboard.ENTER:vpFocusEnter = true;vpFocusEvent=true;vpOnEnter=true;
					if(vpVarCurrent != txt.text && txt.text != ''){
						F_cari(txt.id);
					}else if(vpVarCurrent == txt.text)
						focusMgr();
					break;				
			}
		}
		function f_cariklik(img:Image, txt:Object):void{
			img.addEventListener(MouseEvent.CLICK, f_kliklistener);
			function f_kliklistener(e:MouseEvent):void{
				txt.setFocus();
				F_popUpCari(txt.id);
			} 
		}
	}
}

public function F_txtPencarianKhusus(arr:Array):void{
	for(i=0;i<arr.length;i++)
		add(arr[i])
	
	function add(txt:Object):void{
		txt.addEventListener(KeyboardEvent.KEY_DOWN, f_keydown);
		
		m.img = new Image();
		m.img.left = (txt.left+txt.width+3);
		m.img.top = (txt.top+3); 
		m.img.source = tbfind; 
		f_cariklik(m.img, txt);
		txt.parent.addElement(m.img); 
		
		function f_keydown(e:KeyboardEvent):void{
			switch(e.keyCode){
				case Keyboard.F12:vpFocusEnter = true;vpFocusEvent=true;F_popUpCari(txt.id);break;
				case Keyboard.UP:focusMgr(true);break;
				case Keyboard.DOWN:focusMgr();break;
				case Keyboard.ENTER:focusMgr();break;
			}
		}
		function f_cariklik(img:Image, txt:Object):void{
			img.addEventListener(MouseEvent.CLICK, f_kliklistener);
			function f_kliklistener(e:MouseEvent):void{
				txt.setFocus();
				F_popUpCari(txt.id);
			} 
		}
	}
}

public function F_validasiFalse(target:String):Boolean{
	ob = m[idmod].child;
	if(vpFocusEvent == false){
		ob.f_kosongkanText(target);
		//		vpVarCurrent = vpIdTxt.text	;
	}else{
		if(m.bukacompencarian == false)
			if(ob.f_filter(target)[0] != '')
				F_popUpCari(target);
	}
	vpOnEnter = false;
	return vpFocusEvent;
}

public function F_validasiTrue():void{
	if(wsSuccess == true){
		if(vpFocusEnter == true)
			focusMgr();
		vpFocusEnter = false;
	}
}

public function F_csTrue():void
{
	if(vpIdTxt != null)
		vpVarCurrent = vpIdTxt.text;
	F_csFalse();
}


public function F_csFalse():void
{
	vpFocusEnter = false;
}

public function F_cari(target:String):void
{
	var f:Array;
	ob = m[idmod].child;
	if(ob.hasOwnProperty("f_filter")){
		f = ob.f_filter(target);
		if(f[0] != ''){
			m.rowCari = rowIndex;
			F_wsSearch(target, f[0], f[3], f[4], f[1], f[2]);
		}
	}
}

public function F_popUpCari(subject:String, kondisi:String = ''):void{
	f_refreshIDmodule();
	if(m[idmod].child.hasOwnProperty('f_filter')){
		var f:Array = m[idmod].child.f_filter(subject);
		if(f[0] != '')
			F_cariDataCombo(f[0], f[5], f[2], subject, f[7], kondisi);
	}
}	
// --------------------------------- Akhir Pencarian ----------------------

// --------------------------------- Datagrid ----------------------
protected var vgColumnStart:int, vgRowDel:int, vgRowCek:int;
public var vgDown:Boolean = false, vgRollOut:Boolean=false, vgInsertAktif:Boolean = false, vgMyCol:int, vgMyRow:int, vgRowInsert:int;	
public var columnIndex:int, rowIndex:int, vUp:Boolean = false;
public function F_standartKolomGrid(dg:DataGrid, pSkip:Array, pEdit:Array, pTampil:Array, pWajib:Array, pTipe:Array):void
{
	var bTampil:Boolean, bEdit:Boolean, bSkip:Boolean, bWajib:Boolean, t:String, vAc:ArrayCollection = new ArrayCollection;
	for(i=0;i<dg.columnsLength;i++)
		with(dg.columns.getItemAt(i)){
			bTampil = true;
			bEdit = true;
			bSkip = true;
			bWajib = true;
			if(pTipe[i] == 0){
			}else if(pTipe[i] == 1){
				bEdit = false;
			}else if(pTipe[i] == 2){
				bTampil = false;
				bEdit = false;
				bSkip = false;
				bWajib = false;
			}
			if(pTipe[i] == 0)t = 'Normal';
			else if(pTipe[i] == 1)t = 'Info';
			else if(pTipe[i] == 2)t = 'Wajib';
		}
	
	dg.columns.removeAll();
}

public function F_setGridJson(mod:int, menu:int):void
{
	for(i=0;i<m.grid.form.length;i++)
		if(m.grid.form[i].n == mod+"_"+menu){
			wsJson = m.grid.form[i];
			return;
		}
	wsJson = null
}
public function F_RegGrid(mod:int, menu:int, arr:Array, typeGrid:String = '', lockedColumn:int = 0, data:String = ''):void
{
	F_setGridJson(mod, menu);
	registerGrid(arr, typeGrid, lockedColumn, data);
}
public function registerGrid(arr:Array, typeGrid:String = '', lockedColumn:int = 0, data:String = ''):void
{
	var n:int, j:int, x:int, o:Object, obj:Object, om:Object = {};
	f_refreshIDmodule(arr[0]); 
	for(n=0;n<arr.length;n++)
		for(j=0;j<wsJson.grid.length;j++){
			if(arr[n].id == wsJson.grid[j].id){
				if(lockedColumn > 0){
					if(data == '')
						obj = m[module].child[arr[n].id+"_lock"];
					else
						obj = m[module].child[data][arr[n].id+"_lock"];
					obj.columns = new ArrayList;
					obj.setStyle("alternatingRowColors", ['#FFFFFF', '#EFF3FA']);
					om.skip_l = [];om.type_l = [];om.labf_l = [];
				}
				arr[n].columns = new ArrayList;
				arr[n].setStyle("alternatingRowColors", ['#FFFFFF', '#EFF3FA']);
				om.skip = [];om.type = [];om.labf = [];
				for(x=0;x<wsJson.grid[j].property.length;x++){
					o = wsJson.grid[j].property[x];
					
					// Item Editor
					switch(o.ie){
						case '0':om.ie = undefined;break;
						case '1':om.ie = new ClassFactory(ie_nominal);break;
						case '2':om.ie = new ClassFactory(ie_date);break;
						case '3':om.ie = new ClassFactory(ie_discount);break;
						case '4':om.ie = new ClassFactory(ie_number);break;
						case '5':om.ie = new ClassFactory(ie_numericStepper);break;
						case '6':om.ie = new ClassFactory(ie_search);break;
						case '7':om.ie = new ClassFactory(ie_comboBox);break;
						default:om.ie = undefined;
					}
					
					// Label Function
					switch(o.lf){
						case '0':om.lf = F_formatDefaultString;break;
						case '1':om.lf = F_formatCurreny;break;
						case '2':om.lf = F_formatJml;break;
						case '3':om.lf = F_formatDate;
							om.sc = date_sortCompareFunc;
							break;
						case '4':om.lf = F_format1Baris;break;
						case '5':om.lf = F_formatDateTime;
							om.sc = date_sortCompareFunc;
							break;
						default:om.lf = F_formatDefaultString;
					}
					
					// Header Renderer 
					switch(o.hr){
						case '0':om.hr = vLeftHeader;break;
						case '1':om.hr = vCenterHeader;break;
						case '2':om.hr = vRightHeader;break;
						case '3':om.hr = new ClassFactory(ir_checkBoxHeader);break; 
						default:om.hr = vLeftHeader;
					}
					
					// Item Renderer
					switch(o.ir){
						case '0':om.ir = vLeftItem;break;
						case '1':om.ir = vCenterItem;break;
						case '2':om.ir = vRightItem;break;
						case '3':om.ir = new ClassFactory(ir_checkBox);break;
						case '4':om.ir = new ClassFactory(ir_comboBox);break;
						case '5':om.ir = new ClassFactory(ir_link);break;
						case '6':om.ir = new ClassFactory(ir_radioButton);break;
						case '7':om.ir = new ClassFactory(ir_search);break;
						case '8':om.ir = new ClassFactory(ir_date);break;
						case '9':om.ir = new ClassFactory(ir_country);break;
						case '10':om.ir = new ClassFactory(ir_yesno);break;
						case '11':om.ir = new ClassFactory(ir_uploadFile);break;
						case '12':om.ir = new ClassFactory(ir_config);break;
						case '13':om.ir = new ClassFactory(ir_download);break;
						default:om.ir = vLeftItem;
					}
					o.e = int(o.e);
					o.v = int(o.v); 
					if(x < lockedColumn && lockedColumn > 0){
						om.skip_l[om.skip_l.length] = int(o.s);
						om.type_l[om.type_l.length] = o.r;
						om.labf_l[om.labf_l.length] = int(o.lf_l);
						obj.columns.addItem(F_tambahKolomGrid(o.w, o.df, l(o.ht), o.e, o.v, om.lf, om.hr, om.ir, om.ie, om.sc));
					}else{
						om.skip[om.skip.length] = int(o.s);
						om.type[om.type.length] = o.r;
						om.labf[om.labf.length] = int(o.lf);
						arr[n].columns.addItem(F_tambahKolomGrid(o.w, o.df, l(o.ht), o.e, o.v, om.lf, om.hr, om.ir, om.ie, om.sc));
					}	
				}
				m.mod[module]["s"+arr[n].id] = om.skip;
				m.mod[module]["t"+arr[n].id] = om.type;
				m.mod[module]["lf"+arr[n].id] = om.labf;
				if(typeGrid == 'data2'){
					if(lockedColumn > 0){
						f_standartDataGrid2(arr[n]);
						f_standartDataGrid2((obj as DataGrid));
						if(data == ''){
							arr[n].dataProvider = m[module].child[arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[arr[n].id+"_ac"];
						}else{
							arr[n].dataProvider = m[module].child[data][arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[data][arr[n].id+"_ac"];
						}
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else
						f_standartDataGrid2(arr[n]);
				}else if(typeGrid == 'transaksi'){
					if(lockedColumn > 0){
						if(data == ''){
							arr[n].dataProvider = m[module].child[arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[arr[n].id+"_ac"];
						}else{
							arr[n].dataProvider = m[module].child[data][arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[data][arr[n].id+"_ac"];
						}
						m.mod[module]["lf"+obj.id] = om.labf_l;
						F_standartGrid((obj as DataGrid), om.skip_l, om.type_l, false);
						F_standartGrid(arr[n], om.skip, om.type, false);
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else
						F_standartGrid(arr[n], om.skip, om.type);
				}else if(typeGrid == 'data')
					f_standartDataGrid(arr[n]);
				break;
			}
		}
	
	function date_sortCompareFunc(itemA:Object, itemB:Object, itemC:Object):int
	{
		var dateA:Date = new Date(String(itemA.tgl).substr(6,4), String(itemA.tgl).substr(3,2), String(itemA.tgl).substr(0,2));
		var dateB:Date = new Date(String(itemB.tgl).substr(6,4), String(itemB.tgl).substr(3,2), String(itemB.tgl).substr(0,2));
		
		return ObjectUtil.dateCompare(dateA, dateB);
	}
} 

public function F_language(data:String, type:int = 0):String
{
	for(i=0;i<m.bahasa.length;i++)
		if(m.bahasa[i].s == data){
			if(m.bahasa[i].t == '')
				return data;
			else
				return m.bahasa[i].t;
		}
	
	//if(m.languageSystemEnabled == false)
		//return data;
	
	for(i=0;i<m.SentenceSearch.length;i++)
		if(m.SentenceSearch.getItemAt(i).f1 == data)
			return data;
	if(m.NewSentence == undefined)m.NewSentence = new ArrayCollection;
	m.NewSentence.addItem({d:data, t:type});
	m.SentenceSearch.addItem({f1:data});
	return data;
}

public function f_langLabel(a:Array):void
{
	for(i=0;i<a.length;i++){
		a[i].text = l(a[i].text);
	}
}

public function l(data:String, type:int = 0):String
{
	return F_language(data, type);
}

protected var statusEdit:Boolean = false;

public function F_GridLockedColumn(dg:DataGrid, dg2:DataGrid):void
{
	var b1:Boolean = false, b2:Boolean = false;
	dg.addEventListener(flash.events.FocusEvent.FOCUS_IN, f_FOCUS_IN);
	dg.addEventListener(flash.events.FocusEvent.FOCUS_OUT, f_FOCUS_OUT);
	dg.addEventListener(flash.events.Event.RENDER, f_RENDER);
	dg2.addEventListener(flash.events.FocusEvent.FOCUS_IN, f_FOCUS_IN2);
	dg2.addEventListener(flash.events.FocusEvent.FOCUS_OUT, f_FOCUS_OUT2);
	dg2.addEventListener(flash.events.Event.RENDER, f_RENDER2);
	
	function f_FOCUS_IN():void
	{
		b1 = true;
	}
	
	function f_FOCUS_OUT():void
	{
		b1 = false;
	}
	
	function f_RENDER():void
	{
		if(b1 == true){
			if(dg.selectedCell == null && dg2.selectedCell == null){
				dg.setSelectedCell(0, 0);
				dg2.setSelectedCell(0, 0);
			}else if(dg.selectedCell == null)
				dg.setSelectedCell(dg2.selectedCell.columnIndex, 0);
			else if(dg2.selectedCell == null)
				dg2.setSelectedCell(dg.selectedCell.rowIndex, 0);
			else dg2.setSelectedCell(dg.selectedCell.rowIndex, dg2.selectedCell.columnIndex);
			dg2.scroller.verticalScrollBar.value = dg.scroller.verticalScrollBar.value;
		}
	}
	
	function f_FOCUS_IN2():void
	{
		b2 = true;
	}
	
	function f_FOCUS_OUT2():void
	{
		b2 = false;
	}
	
	function f_RENDER2():void
	{
		if(b2 == true){
			if(dg.selectedCell == null && dg2.selectedCell == null){
				dg.setSelectedCell(0, 0);
				dg2.setSelectedCell(0, 0);
			}else if(dg.selectedCell == null)
				dg.setSelectedCell(dg2.selectedCell.rowIndex, 0);
			else if(dg2.selectedCell == null)
				dg2.setSelectedCell(dg.selectedCell.rowIndex, 0);
			else dg.setSelectedCell(dg2.selectedCell.rowIndex, dg.selectedCell.columnIndex);
			dg.scroller.verticalScrollBar.value = dg2.scroller.verticalScrollBar.value;
		}
	}
}

public function f_standartDataGrid(dg:DataGrid):void
{
	with(dg){
		editable = false;
		sortableColumns = false;
		resizableColumns = false;
		dataProvider = new ArrayCollection;
		selectionMode = "singleCell";
	}
	
	dg.addEventListener(KeyboardEvent.KEY_DOWN, f_GRID_KEY_DOWN);
	
	function f_GRID_KEY_DOWN(e:KeyboardEvent):void
	{
		switch(e.keyCode){
			case Keyboard.C:
				flash.system.System.setClipboard(dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[dg.columns.getItemAt(dg.selectedCell.columnIndex).dataField]);
				break;
		}
	}
	
}

public function f_standartDataGrid2(dg:DataGrid):void
{
	with(dg){
		editable = true;
		sortableColumns = false;
		resizableColumns = false;
		dataProvider = new ArrayCollection;
		selectionMode = "singleCell";
	}
}

public function F_OpenNotes(Sumber:String, idTransaksi:int, notransaksi:String):void{
	openFormX("Catatan - "+m.ArrTab[MySuperTab.selectedIndex].label+" - "+notransaksi,1, 'mod/m0/m0_Notes.swf', 350, 500, 't1', Sumber+"|"+idTransaksi)
}

public function F_OpenNotesMaster(Sumber:String, Nama:String, ID_1:String, ID_2:String = ''):void{
	openFormX("Catatan - "+m.ArrTab[MySuperTab.selectedIndex].label+" - "+Nama,1, 'mod/m0/m0_Notes.swf', 350, 500, 't1', Sumber+"|master|"+ID_1+"|"+ID_2)
}

public function F_OpenUploadFile(sumber:String, idtransaksi:int, notransaksi:String):void{
	openFormX("Files - "+m.ArrTab[MySuperTab.selectedIndex].label+" - "+notransaksi, 1, 'mod/m0/m0_Files.swf', 400, 700, undefined, 'data', [sumber, idtransaksi])
}
public function F_OpenFilesMaster(sumber:String, Nama:String, ID_1:String, ID_2:String = ''):void{
	openFormX("Files - "+m.ArrTab[MySuperTab.selectedIndex].label+" - "+Nama, 1, 'mod/m0/m0_Files.swf', 400, 700, undefined, 'master', [sumber, ID_1, ID_2])
}

public function F_standartGrid(dg:DataGrid, skip:Array, type:Array, dp:Boolean = true):void
{
	var vCMI:ContextMenuItem;
	f_refreshIDmodule(dg);
	if(dp == true)
		dg.dataProvider = new ArrayCollection;
	with(dg){
		editable = true;
		sortableColumns = false;
		resizableColumns = false;
		selectionMode = "singleCell";
	}
	rowIndex = 0;
	columnIndex = 0;
	//F_acAdd(dg)
	dg.dataProvider.addItem({no:dg.dataProviderLength+1});
	F_dgColumnFirst(dg);

	ob = m[module].child;
	dg.addEventListener(GridItemEditorEvent.GRID_ITEM_EDITOR_SESSION_CANCEL, f_GRID_ITEM_EDITOR_SESSION_CANCEL);
	dg.addEventListener(GridItemEditorEvent.GRID_ITEM_EDITOR_SESSION_START, f_GRID_ITEM_EDITOR_SESSION_START);
	dg.addEventListener(GridItemEditorEvent.GRID_ITEM_EDITOR_SESSION_SAVE, f_GRID_ITEM_EDITOR_SESSION_SAVE);
	dg.addEventListener(GridEvent.GRID_ROLL_OVER, f_GRID_ROLL_OVER);
	dg.addEventListener(GridEvent.GRID_ROLL_OUT, f_GRID_ROLL_OUT);
	dg.addEventListener(KeyboardEvent.KEY_DOWN, f_GRID_KEY_DOWN);
	dg.addEventListener(MouseEvent.CLICK, f_CLICK);
	
	dg.contextMenu = new ContextMenu;
	dg.contextMenu.hideBuiltInItems();
	dg.contextMenu.addEventListener(ContextMenuEvent.MENU_SELECT, f_dgMENU_SELECT);
	vCMI = new ContextMenuItem('Insert')
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgInsert); 
	
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem('Tambah')      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgTambah); 
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem('Hapus')      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgHapus);  
	dg.contextMenu.customItems.push(vCMI);
	 
	function f_GRID_ITEM_EDITOR_SESSION_START(e:GridItemEditorEvent):void
	{
		vpVarCurrent = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[dg.columns.getItemAt(dg.selectedCell.columnIndex).dataField];
		vgColumnStart = dg.selectedCell.columnIndex;
	}
	
	function f_GRID_ITEM_EDITOR_SESSION_CANCEL(e:GridItemEditorEvent):void
	{
		statusEdit = false;
	}
	
	function f_GRID_ROLL_OVER(e:GridEvent):void
	{
		vgMyRow = e.rowIndex;
		vgMyCol = e.columnIndex;
		vgRollOut = false;
	}
	
	function f_GRID_ROLL_OUT(e:GridEvent):void
	{
		if(vgRollOut == false)
			vgRollOut = true;
	}
	
	function f_GRID_ITEM_EDITOR_SESSION_SAVE():void
	{
		f_refreshIDmodule();
		vpFocusEvent = true;
		columnIndex = dg.selectedCell.columnIndex;
		if(rowIndex <= dg.dataProviderLength-1 && m.focusComboBox)
			if(vpVarCurrent != dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField]){
				if(ob.hasOwnProperty("f_filter")){
					if(ob.f_filter([dg.columns.getItemAt(columnIndex).dataField])[0] != 'CdM1_Transaction_Note_Detail'){
						F_cari(dg.columns.getItemAt(columnIndex).dataField);
					}
				}
				if(m.mod[idmod]['lf'+dg.id][columnIndex] == 1)
					dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField] = dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField].split(m.nfNominal.decimalSeparator)['join']('.');
				if(ob.hasOwnProperty('f_grid'))ob.f_grid(dg, dg.columns.getItemAt(columnIndex).dataField);
			}/*else f_skip(dg);*/
		statusEdit = false;
	} 
	
	function f_GRID_KEY_DOWN(e:KeyboardEvent):void
	{
		vpFocusEvent = true;
		var Right:Boolean = false;
		var Left:Boolean = false;
		var colLength:int = dg.columnsLength-1;
		var rowLength:int = dg.dataProviderLength-1;
		if(dg.selectedCell == null)
			return; 
		
		rowIndex = dg.selectedCell.rowIndex;	
		vgRowCek = dg.selectedCell.rowIndex;
		columnIndex = dg.selectedCell.columnIndex;					
		
		vpVarCurrent = dg.dataProvider.getItemAt(rowIndex)[dg.columns.getItemAt(columnIndex).dataField];
		if(statusEdit == true)
			dg.startItemEditorSession(rowIndex, columnIndex);
		else if(e.altKey){
		}else if(e.ctrlKey)
			switch(e.keyCode){
				case Keyboard.C:
					m.getClipboard = vpVarCurrent;
					flash.system.System.setClipboard(vpVarCurrent);
					break;
//				case Keyboard.D:
//					dg.dataProvider.getItemAt(rowIndex)[dg.columns.getItemAt(columnIndex).dataField] = '';
//					F_dgColumnRefresh(dg, rowIndex);
//					break;
				case Keyboard.E:
					if(F_NumToChar(e.keyCode) != "" && dg.columns.getItemAt(columnIndex).editable == true)
						statusEdit = dg.startItemEditorSession(rowIndex, columnIndex);
					break;
				case Keyboard.R:
					F_dgRefresh(dg);
					break;
				case Keyboard.V:
//					if(m.getClipboard != undefined){
//						dg.dataProvider.getItemAt(rowIndex)[dg.columns.getItemAt(columnIndex).dataField] = m.getClipboard;
//						F_dgColumnRefresh(dg, rowIndex);
//					}
					break;
			}
		else
			switch(e.keyCode){
				case Keyboard.UP:
					if(rowIndex == 0){
						if(vUp == true)focusMgr(true);
						else vUp = true;
					}
					if(dg.dataProviderLength-1 > rowIndex){				
						if(vgInsertAktif == true){
							if(F_acGetPrimaryKey(dg, vgRowInsert, type) == true){
								if(vgRowInsert < dg.dataProviderLength)dg.dataProvider.removeItemAt(vgRowInsert);
								for(i=vgRowInsert;i<dg.dataProviderLength;i++)
									F_acSet(dg, int(dg.dataProvider.getItemAt(i).no)-1, i, i);
								vgInsertAktif = false;
							}else vgDown = false;
						}else vgDown = false;
					}else vgDown = false;		
					F_hapusKhusus(dg);
					break;
				case Keyboard.DOWN:
					F_hapusKhusus(dg);
					vUp = false;
					if(dg.dataProviderLength-1 == rowIndex && F_acGetPrimaryKey(dg, rowIndex, type) == false){
						if(vgDown == true){
							vgRowInsert = dg.dataProviderLength;
							F_acAdd(dg);
							focusFirst(vgRowInsert);
							vgInsertAktif = true; 
							F_dgscrolling(dg);
							F_autoScrolling(dg, 0);
						}
						vgDown = true;
					}else vgDown = false;							
					break;
				case Keyboard.ENTER:
					f_skip(dg); return;
					break;
				case Keyboard.DELETE:									
					vgRowDel = rowIndex;
					Alert.show('Delete baris ke-'+String(rowIndex+1)+' ?', 'Konfirmasi', 3, null, function(e:Object):void{
						if(e.detail == Alert.YES){
							ob = m[idmod].child;
							dg.dataProvider.removeItemAt(vgRowDel);
							for(i=vgRowDel;i<dg.dataProviderLength;i++)
								F_acSet(dg, int(dg.dataProvider.getItemAt(i).no)-1, i, i);
							if(vgRowDel == 0 && dg.dataProviderLength == 0){
								F_acAdd(dg);
								for(i=0;i<dg.columnsLength;i++)
									if(skip[i] == 0){
										dg.setSelectedCell(0, i);
										break;
									}
							}
							dg.setFocus();
							if(vgRowDel != rowLength)dg.setSelectedCell(vgRowDel, columnIndex);
							else dg.setSelectedCell(vgRowDel-1,columnIndex);
							F_autoScrolling(dg, 0);
							vgInsertAktif = false;
							if(ob.hasOwnProperty('f_grid'))ob.f_grid(dg, 'del');
							if(ob.hasOwnProperty('f_hitungTotal'))ob.f_hitungTotal();
						}
					});							
					break;
				case Keyboard.INSERT:
					F_insertDG(dg, rowIndex, skip);
					break;
				case Keyboard.F12:
					F_popUpCari(dg.columns.getItemAt(columnIndex).dataField);
					break;
				case Keyboard.SPACE:
					if(ob.hasOwnProperty('f_grid'))ob.f_grid(dg, 'space');
					break;
				default:
					if((F_NumToChar(e.keyCode) != "" && dg.columns.getItemAt(columnIndex).editable) || e.keyCode == 189)
						statusEdit = dg.startItemEditorSession(rowIndex, columnIndex);
			}
	}
	
	function f_CLICK():void
	{

		//config
		if(dg.selectedCell != null){
			// untuk jika tekan arah atas biar pas, sesuatu aturan
			if(dg.selectedCell.rowIndex > 0){
				vUp = false;
			}
			
			rowIndex = dg.selectedCell.rowIndex;
//			columnIndex = dg.selectedCell.columnIndex;
		}else return;
//		vpFocusEvent = true;
		vgRowCek=rowIndex;
		// hapus
		F_hapusKhusus(dg);
		if (vgRollOut==true){
			if(F_acGetPrimaryKey(dg, dg.dataProviderLength-1, type) != true){
				vgRowCek = NaN;
				F_hapusKhusus(dg);		
				vgRowInsert = dg.dataProviderLength;
				F_acAdd(dg);
				F_autoScrolling(dg, 0);
				for(i=0;i<dg.columnsLength;i++)
					if(skip[i] == 0){
						dg.setSelectedCell(dg.dataProviderLength-1, i);
						break;
					}
				vgInsertAktif = true;
				vgRollOut = false;
			}
		}
	}
	
	function f_skip(dg:DataGrid):void
	{
		m.column = dg.columns.getItemAt(columnIndex).dataField;
		if(m.focusComboBox){
			if(m[idmod] != null)
				if(ob.hasOwnProperty('f_filter')){
					if(ob.f_filter(m.column)[0] != '' && type[columnIndex] == 1 && (dg.dataProvider.getItemAt(rowIndex)[m.column] == null || dg.dataProvider.getItemAt(rowIndex)[m.column] == '' || dg.dataProvider.getItemAt(rowIndex)[m.column] == 0))
						F_popUpCari(m.column);
					else F_gridSkip(dg, skip)
				}else F_gridSkip(dg, skip)
		}else
			m.focusComboBox = true;
	}
	
	function F_gridSkip(dg:DataGrid, skip:Array):void
	{
		for(i=columnIndex+1;i<dg.columnsLength;i++)
			if(skip[i] == 0){
				columnIndex = i;
				F_autoScrolling(dg, i);
				dg.setSelectedCell(rowIndex, i);
				break;
			}else if(i == dg.columnsLength-1){
				for(i=0;i<dg.columnsLength;i++)
					if(skip[i] == 0){
						if(dg.dataProviderLength-1 == rowIndex){
							if(F_acGetPrimaryKey(dg, rowIndex, type) == false){
								vgRowInsert = dg.dataProviderLength;
								F_acAdd(dg);
								vgInsertAktif = true;
								dg.setSelectedCell(dg.dataProviderLength-1, i);
							}else dg.setSelectedCell(rowIndex, i);
						}else
							dg.setSelectedCell(rowIndex+1, i);
						F_autoScrolling(dg, 0);
						F_dgscrolling(dg);
						break;
					}
				break;
			}
	}
	
	function F_hapusKhusus(dg:DataGrid):void
	{
		if(vgInsertAktif == true){
			if(F_acGetPrimaryKey(dg, vgRowInsert, type) == true && vgRowInsert != vgRowCek && dg.dataProviderLength > vgRowInsert){
				dg.dataProvider.removeItemAt(vgRowInsert);
				for(i=vgRowInsert;i<dg.dataProviderLength;i++)
					F_acSet(dg, int(dg.dataProvider.getItemAt(i).no)-1, i, i);	
				vgInsertAktif = false;
			}					
		}
	}
	
	function f_dgMENU_SELECT():void
	{
		m.vgMyRow = vgMyRow;
		m.vgMyCol = vgMyCol;
		dg.setSelectedCell(m.vgMyRow, m.vgMyCol);
		dg.validateNow();
	}
	
	function f_dgInsert():void
	{
		F_insertDG(dg, m.vgMyRow, skip)
	}
	
	function f_dgTambah():void
	{
		F_hapusKhusus(dg);
		vgRowInsert = dg.dataProviderLength;
		F_acAdd(dg);
		for(i=0;i<dg.columnsLength;i++)
			if(skip[i] == 0){
				dg.setSelectedCell(dg.dataProviderLength-1, i);
				break;
			}
	}
	
	function f_dgHapus():void
	{
		ob = m[idmod].child;
		F_hapusKhusus(dg);
		dg.dataProvider.removeItemAt(m.vgMyRow);
		for(i=m.vgMyRow;i<dg.dataProviderLength;i++){
			F_acSet(dg, int(dg.dataProvider.getItemAt(i).no)-1, i, i);
		}
		if(vgRowDel == 0 && dg.dataProviderLength == 0){
			F_acAdd(dg);
			for(i=0;i<dg.columnsLength;i++)
				if(skip[i] == 0){
					dg.setSelectedCell(0, i);
					break;
				}
		}
		if(ob.hasOwnProperty('f_hitungTotal'))ob.f_hitungTotal();
		dg.setSelectedCell(m.vgMyRow, m.vgMyCol);
		vgInsertAktif = false;
	}
	
	function F_insertDG(dg:DataGrid, row:int, skip:Array):void
	{
		F_acAdd(dg, row);
		for(i=row+1;i<dg.dataProviderLength;i++){
			dg.dataProvider.getItemAt(i).no = (i+1);
			F_dgColumnRefresh(dg, i)
		}
		F_autoScrolling(dg, 0);
		focusFirst(row);
	}			
	
	function focusFirst(row:int):void
	{
		for(i=0;i<dg.columnsLength;i++){
			if(skip[i] == 0){
				dg.setSelectedCell(row, i);
				break;
			}
		}
	}
	
	function F_autoScrolling(dg:DataGrid, w:int):void
	{	
		var n:int=dg.scroller.horizontalScrollBar.maximum, i:int;
		if(w > 0){
			for(i=0;i<w;i++){
				if(dg.columns.getItemAt(i).visible == true){
					n -= dg.columns.getItemAt(i).width;
					if(n < 0){
						n=dg.scroller.horizontalScrollBar.maximum
						for(i=dg.columnsLength-1;i>w;i--)
							if(dg.columns.getItemAt(i).visible == true)
								n -= dg.columns.getItemAt(i).width;
						dg.scroller.horizontalScrollBar.value = n;
						return;
					}
				} 
			}
			n=dg.scroller.horizontalScrollBar.maximum
			for(i=dg.columnsLength-1;i>w;i--)
				if(dg.columns.getItemAt(i).visible == true)
					n -= dg.columns.getItemAt(i).width;
			dg.scroller.horizontalScrollBar.value = n;
		}else
			dg.scroller.horizontalScrollBar.value = 0;
	}
	
}

public function F_dgColumnFirst(dg:DataGrid, row:int = -1):void
{
	f_refreshIDmodule(dg);
	var skip:Array = m.mod[module]['s'+dg.id], i:int;
	if(skip != null){
		if(row == -1)row = rowIndex;
		else rowIndex = row;
		for(i=0;i<dg.columnsLength;i++)
			if(skip[i] == 0){
				columnIndex = i;
				dg.setSelectedCell(row, i);
				break;
			}
	}	
}

public function F_dgscrolling(dg:DataGrid):void
{	
	var n:int=0, i:int;
	if(dg.dataProviderLength > 0)
		for(i=0;i<dg.dataProviderLength;i++)
			n += dg.rowHeight;
	else n=0; 
	if(dg.scroller != null){
		dg.scroller.validateNow();
		dg.scroller.verticalScrollBar.value = n-(int(dg.height)-40);
	}
}

public function F_acAdd(dg:DataGrid, atRow:int = -1):void
{	
	dg.setFocus();
	if(atRow < 0){
		dg.dataProvider.addItem({no:dg.dataProviderLength+1});
		if(m[idmod].child != null)
		if(m[idmod].child.hasOwnProperty('f_grid'))m[idmod].child.f_grid(dg, 'add');
		F_dgscrolling(dg);
		if(dg.selectedCell != null)F_dgColumnFirst(dg, dg.dataProviderLength-1);
	}else{
		dg.dataProvider.addItemAt({no:atRow+1}, atRow);
		if(m[idmod].child != null)
		if(m[idmod].child.hasOwnProperty('f_grid'))m[idmod].child.f_grid(dg, 'add|'+atRow);
		if(dg.selectedCell != null)F_dgColumnFirst(dg, atRow);
	}
}

public function F_acSet(dg:DataGrid, nolama:int, lama:int, baru:int):void{
	dg.dataProvider.getItemAt(lama).no = nolama;
	dg.dataProvider.setItemAt(dg.dataProvider.getItemAt(lama), baru);
}

public function F_acGetPrimaryKey(dg:DataGrid, no:int, type:Array):Boolean
{ 
	if(no < dg.dataProviderLength){
		for(j=0;j<type.length;j++)
			if(type[j] == 1 && (dg.dataProvider.getItemAt(no)[dg.columns.getItemAt(j).dataField] == null || dg.dataProvider.getItemAt(no)[dg.columns.getItemAt(j).dataField] == '' || dg.dataProvider.getItemAt(no)[dg.columns.getItemAt(j).dataField] == 0 || dg.dataProvider.getItemAt(no)[dg.columns.getItemAt(j).dataField] == undefined))
				return true;
		return false;
	}else return true;
}

public function F_hapusBarisKosongGrid(dg:DataGrid, type:Array = null):void
{
	if(type == null)
		type = m.mod[idmod]['t'+dg.id];
	var u:int;
	for(i=dg.dataProviderLength-1;i>=0;i--)
		if(F_acGetPrimaryKey(dg, i, type) == true)
		{ 
			dg.dataProvider.removeItemAt(i); 
			for(u=i;u<dg.dataProviderLength;u++)
				F_acSet(dg, dg.dataProvider.getItemAt(u).no-1, u, u);
		}
	if(dg.dataProviderLength == 0)F_acAdd(dg);
	rowIndex = dg.dataProviderLength;
	F_dgColumnFirst(dg, dg.dataProviderLength-1);
	vgInsertAktif = false;
	dg.endItemEditorSession(false);
}

public function F_defaultGridItem(dg:DataGrid):void
{
	dg.dataProvider.removeAll();
	F_acAdd(dg); 
	dg.setSelectedCell(0, 0);
	vgRowInsert = 0;
}

public function F_selectCell(dg:DataGrid, dataField:String):void
{
	for(i=0;i<dg.columnsLength;i++)
		if(dg.columns.getItemAt(i).dataField == dataField)
			break;
	dg.setSelectedCell(dg.selectedCell.rowIndex, i);
}
// --------------------------------- Akhir Datagrid ----------------------

// --------------------------------- Fungsi-fungsi untuk modul ------------------------------------

public function F_tglFormatDB(s:String):String
{
	var arr:Array = s.split("/"), d:String;
	if(arr.length==3)
		if(arr[2].split(' ')['length'] > 1)
			d = arr[2].split(' ')[0]+"-"+arr[1]+"-"+arr[0]+" "+arr[2].split(' ')[1];
		else
			d = arr[2]+"-"+arr[1]+"-"+arr[0];
	else 
		d = '';
	return d;
}

public function F_unCurreny(x:String):Number
{
	if(m.formatNominal[0] == "," && m.formatNominal[1] == ".")
		return Number(x.split(',').join(''));
	if(m.formatNominal[0] == "." && m.formatNominal[1] == ",")
		return Number(x.split('.').join('').split(',').join('.'));
	return 0;
}

public function F_formatDefaultString(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == undefined)
		item[column.dataField] = '';
	return item[column.dataField];
}		

public function F_format1Baris(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == undefined)
		item[column.dataField] = '';
	m.s = item[column.dataField].split('\n')[0];
	if(item[column.dataField].split('\n')['length'] > 1)
		return m.s.slice(0, m.s.length-1);
	else
		return item[column.dataField];
}		

public function F_formatJml(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == '' || item[column.dataField] == undefined)
		item[column.dataField] = 0;
	return m.nfNumber.format(item[column.dataField]);
}		

public function F_formatCurreny(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == '' || item[column.dataField] == undefined)
		item[column.dataField] = 0;
	
	item[column.dataField] = String(item[column.dataField]).split(m.nfNominal.decimalSeparator).join('.');
	m.temp = item[column.dataField].split('+');
	if(m.temp.length > 1){
		// Jika dison Bertingkat 
		m.s = m.nfNominal.format(m.temp[0]);
		for(i=1;i<m.temp.length;i++)
			m.s += "+"+m.nfNominal.format(m.temp[i]);
		return m.s;
	}else
		return m.nfNominal.format(item[column.dataField]);
}

public function F_formatNominal(data:String):String
{
	if(data == null || data == '')
		data = '0';
	
	data = data.split(m.nfNominal.decimalSeparator).join('.');
	m.temp = data.split('+');
	if(m.temp.length > 1){
		// Jika dison Bertingkat 
		m.s = m.nfNominal.format(m.temp[0]);
		for(i=1;i<m.temp.length;i++)
			m.s += "+"+m.nfNominal.format(m.temp[i]);
		return m.s;
	}else
		return m.nfNominal.format(data);
}

public function F_formatDiskon(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == '' || item[column.dataField] == undefined)
		item[column.dataField] = 0;
	return m.nfNominal.format(item[column.dataField]);
}

public function F_formatDate(item:Object, column:GridColumn):String
{
	var s:String;
	if(item[column.dataField] == null || item[column.dataField] == ''){
		s = DefaultTanggal;
//		s = m.DateFirst;
		item[column.dataField] = s;
	}else{
		//variable pendukung
		var dt:String = item[column.dataField];
		var a:Array = DefaultTanggal.split("/"), d:Array;

		//distandarkan format tanggalnya
		dt = dt.split(" ").join("/");
		dt = dt.split("-").join("/");
		d = dt.split('/');
		//Default Hari ini
		var sd:String = DefaultTanggal;
		
		if(d.length == 1)
			switch(dt.length){
				case 1:f_valid("0"+dt, a[1], a[2]);break;
				case 2:f_valid(dt, a[1], a[2]);break;
				case 4:f_valid(dt.slice(0,2), dt.slice(2,4), a[2]);break;
				case 6:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,6));break;
				case 8:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,8));break;
				default:item[column.dataField] = '';break;
			}
		else if(d.length == 2)
			f_valid(d[0], d[1], a[2]);
		else if(d.length == 3)
			f_valid(d[0], d[1], d[2]);
		else item[column.dataField] = '';
		
		function f_valid(dd:int, mm:int, yyyy:int):void
		{
			var d:int, d1:String, d2:String;
			if(mm > 12)mm = 12;
			else if(mm < 1)mm = 1;
			if(yyyy == 0)yyyy = 1900;
			else if(String(yyyy).length < 4)yyyy = 1900;
			else if(yyyy > 2100) yyyy = 2100;
			d = FlexGlobals.topLevelApplication.F_getDayCount(yyyy, mm);
			if(dd > d) dd = d;
			else if(dd < 1) dd = 1;
			d1 = String(dd);
			d2 = String(mm);
			if(d1.length == 1)d1 = "0"+d1;
			if(d2.length == 1)d2 = "0"+d2;
			item[column.dataField] = d1+"/"+d2+"/"+yyyy;
		}		
		s = item[column.dataField];
	}
	return s;
}

public function F_formatDateTime(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == '')
		item[column.dataField] = m.DefaultDT;
	return item[column.dataField];
}

public var grid:DataGrid;
public function F_dgKolom(width:int = 0, dataField:String = '', headerText:String = '', labelFunction:Function = undefined, headerRenderer:IFactory = undefined, itemRenderer:ClassFactory = undefined, ItemEditor:ClassFactory = undefined):void
{ 
	grid.columns.addItem(F_tambahKolomGrid(width, dataField, headerText, true, true, labelFunction, headerRenderer, itemRenderer, ItemEditor));
}

public function F_tambahKolomGrid(width:int = undefined, dataField:String = '', headerText:String = '', editable:Boolean = true, visible:Boolean = true, labelFunction:Function = undefined,  headerRenderer:IFactory = undefined, itemRenderer:ClassFactory = undefined, ItemEditor:ClassFactory = undefined, sortCompareFunction:Function = undefined):GridColumn
{ 
	var c:GridColumn = new GridColumn();
	if(width >= 0)
	c.width = width;
	c.dataField = dataField;
	c.headerText = headerText;
	c.editable = editable;
	c.visible = visible;
	c.labelFunction = labelFunction;
	c.headerRenderer = headerRenderer;
	c.itemRenderer = itemRenderer; 
	c.itemEditor = ItemEditor;
	c.sortCompareFunction = sortCompareFunction
	return c;
}

public function F_tabvalidatenow(tab:TabNavigator):void
{
	var tabnow:int = tab.selectedIndex, i:int;
	for(i=0;i<tab.length;i++){
		tab.selectedIndex = i;
		tab.validateNow();
	}
	tab.selectedIndex = tabnow;
	tab.validateNow();
}
public function F_splitArrayToString(a:Array):String{
	var s:String, j:int;
	for(j=0;j<a.length;j++){
		if(j==0)s = a[j];
		else s += sptField+a[j];
	}
	return s;
}

public function F_splitWs(a:Array):String{
	var s:String = '', j:int;
	for(j=0;j<a.length;j++){
		if(j==0)s = F_splitArrayToString(a[j]);
		else s += sptRow+F_splitArrayToString(a[j]);
	}
	return s;
}

// Form Data
public function F_persiapanFormData(obj:Object):void
{
	var vSort:Boolean = true, vPosisi:int = 0, vCMI:ContextMenuItem, dg:DataGrid, mb:MenuBar, arr:Array = new Array, sumber:String;
	f_refreshIDmodule(obj);
	F_RegGrid(m[module].child.a[0], m[module].child.a[2], [m[module].child.dg], 'data');
	ob = m[module].child;
	dg = m[module].child.dg;
	m.a = [];
	m.a[0] = m[module].child.a[0];
	m.a[1] = m[module].child.a[1];
	m.s = m[module].child.a[3];
	sumber = m[module].child.a[3].toLowerCase();
	mb = m[module].child.mb;
	ob.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	// Config Paging
	ob.txtpaging.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_pagingKEY_DOWN);
	ob.txtpaging.restrict = "0-9";
	ob.txtpaging.setStyle("textAlign","center");
	ob.txtpaging.text = 0;
	ob.txtpaging.maxChars = 4;
	ob.imgfirst.enabled = false;
	ob.imgprevious.enabled = false;
	ob.imgnext.enabled = false;
	ob.imglast.enabled = false;
	ob.imgfirst.addEventListener(MouseEvent.CLICK, f_firstCLICK);
	ob.imgprevious.addEventListener(MouseEvent.CLICK, f_previousCLICK);
	ob.imgnext.addEventListener(MouseEvent.CLICK, f_nextCLICK);
	ob.imglast.addEventListener(MouseEvent.CLICK, f_lastCLICK);
	
	function f_KEY_DOWN(e:KeyboardEvent):void
	{ 
		switch(e.keyCode){
			case Keyboard.HOME:
				if(int(ob.txtpaging.text)>1)
					f_formDataTampil();
				break;
			case Keyboard.PAGE_UP:
				if(int(ob.txtpaging.text)-1>0)
					f_formDataTampil(int(ob.txtpaging.text)-1);
				break;
			case Keyboard.PAGE_DOWN:
				f_formDataTampil(int(ob.txtpaging.text)+1);
				break;
			case Keyboard.END:
				f_formDataTampil(-1);
				break;
		}
	}
	
	
	function f_firstCLICK():void
	{
		if(int(ob.txtpaging.text)>1)
			f_formDataTampil();
	}
	
	function f_previousCLICK():void
	{
		if(int(ob.txtpaging.text)-1>0)
			f_formDataTampil(int(ob.txtpaging.text)-1);
	}
	
	function f_nextCLICK():void
	{
		f_formDataTampil(int(ob.txtpaging.text)+1);
	}
	
	function f_lastCLICK():void
	{
		f_formDataTampil(int(-1));
	}
	
	function f_pagingKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.keyCode == Keyboard.ENTER)
			if(int(ob.txtpaging.text)>0)
				f_formDataTampil(int(ob.txtpaging.text));
	}
	
	for(i=0;i<m.ArrUserMenu.length;i++)
		if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
			m.o = m.ArrUserMenu.getItemAt(i);
			break;
		}
	
	if(m[module].child.a[4]){
		m[module].child.cmbstatus.dataProvider =  new ArrayCollection([{l:'Semua', v:-1}, {l:'Draft', v:0}, {l:'Need Approval', v:1}, {l:'Approved', v:2}, {l:'In Progress', v:3}, {l:'Complete', v:4}, {l:'Revisi', v:5}, {l:'Reject', v:6}, {l:'Close', v:7}, {l:'Approve 1', v:8}, {l:'Approve 2', v:9}, {l:'Approve 3', v:10}, {l:'Approve 4', v:11}]);
	}else{
		m[module].child.cmbstatus.dataProvider =  new ArrayCollection([{l:'Semua', v:-1}, {l:'Draft', v:0}, {l:'Approved', v:2}, {l:'In Progress', v:3}, {l:'Complete', v:4}, {l:'Close', v:7}]);
	}
	m[module].child.txtlokasi.text = ulokasi;
	
	// Config Pop Up
	m[module].child.gridjv.dataProvider = new ArrayCollection();
	m[module].child.gridtt.dataProvider = new ArrayCollection();
	m[module].child.ttwpopup.addEventListener(mx.events.CloseEvent.CLOSE, f_ttwClose);
	m[module].child.ttwpopup.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_ttwKEY_DOWN);
	
	// Config Grid
	dg.dataProvider = new ArrayCollection();
	dg.addEventListener(MouseEvent.CLICK, f_dgClick);
	dg.addEventListener(GridEvent.GRID_ROLL_OVER, f_dgROLL_OVER);
	dg.addEventListener(GridSortEvent.SORT_CHANGING, f_dgSort_changing);
	dg.selectionMode = "singleCell";
	dg.contextMenu = new ContextMenu;
	dg.contextMenu.hideBuiltInItems();
	dg.contextMenu.addEventListener(ContextMenuEvent.MENU_SELECT, f_MENU_SELECT);
	vCMI = new ContextMenuItem(l('Cetak Transaksi ini'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_cetakDetail);
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem('Transaksi Terkait')
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_transaksiTerkait); 
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem('Jurnal Voucher')      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jurnalVocher); 
	dg.contextMenu.customItems.push(vCMI);
	if(sumber != "pa"){
		vCMI = new ContextMenuItem('Jadikan Draft')      
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jadikanDraft);  
		dg.contextMenu.customItems.push(vCMI);   
	}
	vCMI = new ContextMenuItem('Jadikan Close / Unclose')     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jadikanCloseUnclose);  
	dg.contextMenu.customItems.push(vCMI); 
	vCMI = new ContextMenuItem('Histori Transaksi')     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_historiTransaksi);  
	dg.contextMenu.customItems.push(vCMI);     
	if(sumber != "pa"){
		vCMI = new ContextMenuItem('Hapus')     
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_hapus);  
		dg.contextMenu.customItems.push(vCMI);     
	}  
	// Config Menu Bar
	mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, f_tampilkan); // tampil
	mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, f_reset); // reset
	mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, f_tambah); // tambah
//	mb.menuBarItems[3].addEventListener(MouseEvent.CLICK, f_cetak); // cetak
	mb.addEventListener(mx.events.MenuEvent.ITEM_CLICK, f_mbItemClick); 
	function f_ttwClose():void
	{
		ob = m[idmod].child;
		ob.grppopup.visible = false;
		ob.gridjv.visible = false;
		ob.gridtt.visible = false;
		ob.gridjv.dataProvider.removeAll();
		ob.gridtt.dataProvider.removeAll();
		ob.dg.setFocus();
	}
	
	function f_ttwKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.keyCode == Keyboard.ESCAPE)
			f_ttwClose();
	}
	
	function f_dgClick():void
	{
//		if(dg.dataProviderLength > 0)
//			if(dg.selectedCell.columnIndex == 0)
//				f_edit()
//		Alert.show(dg.selectedCell.rowIndex+" ");
	}
	
	function f_dgROLL_OVER(e:GridEvent):void
	{
		if(dg.dataProviderLength > 0){
			arr[0] = e.rowIndex;
			arr[1] = e.columnIndex;
		}
	}
	
	function f_dgSort_changing(e:GridSortEvent):void
	{
		var om:Object = ob.dg.columns.getItemAt(int(e.columnIndices)).dataField;
		if(vSort == true){
			vSort = false;
			f_formDataTampil(1, String(om));
		}else if(vPosisi != int(e.columnIndices)){
			vSort = false;
			f_formDataTampil(1, String(om));
		}else{
			vSort = true;
			dg.dataProvider.removeAll();
			f_formDataTampil(1, om+' DESC');
		}
	}
	
	function f_MENU_SELECT():void
	{
		arr[2] = arr[0]; //row
		arr[3] = arr[1]; //col
		dg.setSelectedCell(arr[2], arr[3]);
	}
	
	function f_tampilkan():void
	{
		if(int(m[idmod].child.txtpaging.text) == 0)
			f_formDataTampil(1);
		else
			f_formDataTampil(int(m[idmod].child.txtpaging.text));
	}
	
	
	function f_hapus():void
	{
		var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
		var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
		if(dg.dataProviderLength > 0 && apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 3) == true)
			if(status == 0 || status == 5 || status == 6)
				Alert.show(l("Apakah Anda yakin akan menghapus No. Transaksi ini ?", 1), 'Information', 3, null, function(e:Object):void{	
					if(e.detail == Alert.YES)
						F_wsDelete(sptParam+'updateStatus', 'M'+m[idmod].child.a[0]+'_'+m[idmod].child.a[3]+'Delete', userid, id.toString(), int(ob.txtpaging.text), 20, ob.f_filterdata(), vDataSort);
				});
			else Alert.show('Status Draft, Revisi dan Reject yang bisa di hapus', 'Peringatan');
	}
	
	function f_reset():void
	{
		ob.f_kondisiAwal();
	}
	
	function f_tambah():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 1) == true){
			f_refreshIDmodule(obj);
			for(i=0;i<m.ArrUserMenu.length;i++)
				if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
					m.o = m.ArrUserMenu.getItemAt(i);
					break;
				}
			openFormX(m.o.mnname, 0, m.o.mnurl, 0, 0, "t2");
		}
	}
	
	function f_edit():void
	{
		var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 4) == true){
			f_refreshIDmodule(obj);
			for(i=0;i<m.ArrUserMenu.length;i++)
				if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
					m.o = m.ArrUserMenu.getItemAt(i);
					break;
				}
			openFormX(m.o.mnname, 0, m.o.mnurl, 0, 0, "t2", String(id));
		}
	}
	
	function f_cetak():void
	{
		F_openReport(m[idmod].child.a[0], m[idmod].child.a[1], ob.cmbjenislaporan.selectedItem.ritem, ob.f_filterdata());
	}
	
	function f_bantuan():void
	{
		F_help(m[idmod].child.a[0], m[idmod].child.a[1]);
	}
	
	function f_cetakDetail():void
	{ 
		m.o = m[module].child;
		sumber = m.o.a[3].toLowerCase();
		dg = m.o.dg;
		if(m.o.cmbjenislaporan.selectedIndex >= 0){
			m.s = m.o.f_filterdata();
			if(m.s.length == 0)
				m.s = (sumber+"notransaksi = '"+dg.dataProvider[dg.selectedCell.rowIndex][sumber+"notransaksi"]+"'");
			else
				m.s = m.s + "AND ("+sumber+"notransaksi = '"+dg.dataProvider[dg.selectedCell.rowIndex][sumber+"notransaksi"]+"'"+")";
			m.p = F_paramDate(m.o.dttgl1.text, m.o.dttgl2.text);
			F_openReport(m[idmod].child.a[0], m[idmod].child.a[1], m[idmod].child.cmbjenislaporan.selectedItem.ritem, m.s, vDataSort, '', 0, m.p, sumber.toUpperCase(), dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"]);
		}else{
			m[idmod].child.cmbjenislaporan.setFocus();
			Alert.show(l("Pilih Jenis Laporan dulu"), l("Informasi"));
		}
	}
	
	function f_transaksiTerkait():void
	{
		if(dg.dataProviderLength > 0)
			F_wsGetDataById(sptParam+'tampilFDTransaksiTerkait', 'M'+m[idmod].child.a[0]+'_'+m[idmod].child.a[3]+'Terkait', dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"]); 
	}
	
	function f_jurnalVocher():void
	{
		var notransaksi:String = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"notransaksi"];
		if(dg.dataProviderLength > 0 && apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 11) == true)
			F_wsSearch(sptParam+'tampilFDJurnalVocher', 'M2_Transaction_Journal_VoucherSearch', 0, 0, "tnotransaksi = '"+notransaksi+"'"); 
	}
	
	function f_jadikanDraft():void
	{
		var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
		var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
		if(dg.dataProviderLength > 0)
			if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 2) == true)
				if(status == 2)
					F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[idmod].child.a[0]+'_'+m[idmod].child.a[3]+'UpdateStatus', userid, id, '0', false, int(m[idmod].child.txtpaging.text), 20, m[idmod].child.f_filterdata(), vDataSort);
				else Alert.show(l('Hanya status approved yang bisa di jadikan Draft'), l('Peringatan'));
	}
	
	function f_jadikanCloseUnclose():void
	{
		var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
		var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
		var statussebelumnya:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"statussebelumnya"];
		if(dg.dataProviderLength > 0 && apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 10) == true)
			if(status == 7)
				F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[idmod].child.a[0]+'_'+m[idmod].child.a[3]+'UpdateStatus', userid, id, 'unclose', false, int(m[idmod].child.txtpaging.text), 20, m[idmod].child.f_filterdata(), vDataSort);
			else if(status == 2 || status == 3 || status == 4)
				F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[idmod].child.a[0]+'_'+m[idmod].child.a[3]+'UpdateStatus', userid, id, '7', false, int(m[idmod].child.txtpaging.text), 20, m[idmod].child.f_filterdata(), vDataSort);
			else Alert.show('Status Approved, Inprogress dan Complete yang bisa di Close', 'Peringatan');		
	}
	
	function f_historiTransaksi():void
	{
		if(dg.dataProviderLength > 0 && apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 12) == true)
			Alert.show('Histori Transaksi', 'Form Belum Jadi');
	}
	function f_mbItemClick(e:MenuEvent):void
	{
		switch(e.label){
			case 'Bantuan':f_bantuan();
				break;
			case 'Edit':f_edit();
				break;
			case 'Hapus':f_hapus();
				break;
			case 'Cetak Detail':f_cetakDetail();
				break;
			case 'Transaksi Terkait':f_transaksiTerkait();
				break;
			case 'Jurnal Voucher':f_jurnalVocher();
				break;
			case 'Jadikan Draft':f_jadikanDraft();
				break;
			case 'Jadikan Close/Unclose':f_jadikanCloseUnclose();
				break;
			case 'Histori Transaksi':f_historiTransaksi();
				break;
			default:
				m.o = m[idmod].child
				sumber = m.o.a[3].toUpperCase();
				dg = m.o.dg;
				m.p = F_paramDate(m.o.dttgl1.text, m.o.dttgl2.text);
//				Alert.show(sumber+" "+dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"]);
				if(m[idmod].child.cmbjenislaporan.selectedIndex >= 0)
					F_openReport(m[idmod].child.a[0], m[idmod].child.a[1], m[idmod].child.cmbjenislaporan.selectedItem.ritem, m[idmod].child.f_filterdata(), vDataSort, '', e.item.@data, m.p, sumber, dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"]);
				else{
					m[idmod].child.cmbjenislaporan.setFocus();
					Alert.show(l("Pilih Jenis Laporan dulu"), l("Informasi"));
				}
		}
	}
}

protected var vDataSort:String;
public function f_formDataTampil(page:int = 1, sort:String = ''):void
{
	ob = m[idmod].child
	if(sort == '')sort = ob.a[3]+"inputtgl desc";
	vDataSort = sort;
	m.sumber = ob.a[3].toLowerCase();
	F_wsSearch("★tampilForm", 'M'+ob.a[0]+'_'+ob.a[3]+'Search', page, 20, ob.f_filterdata(), sort);
}

// Form Master
public var idmodcurr:String;

public function F_configMasterInput():void
{
	ob = m[idmod].child;
	ob.txtkode.setFocus();
	ob.vUserId = userid;
	ob.vDefDt = DefaultTanggalforDB;
//	ob.arr = ArrFilter;
	ob.addEventListener(KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	ob.mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, function():void{ob.f_simpan(true)}); // simpanbaru
	ob.mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, function():void{ob.f_simpan(false)}); // simpan
	ob.mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, function():void{closeFormPopUp(1)}); // tutup
	
	function f_KEY_DOWN(e:KeyboardEvent):void{
		if(e.ctrlKey && e.keyCode == Keyboard.N){ob.f_simpan(true);
		}else if(e.ctrlKey && e.keyCode == Keyboard.S){ob.f_simpan(false);
		}else if (e.keyCode == Keyboard.ESCAPE){closeFormPopUp(1)}
	}
}

public var vfplus:String='';
public function F_txtInOut(param:Object,fplus:String,paket:String):void
{
	ob = m[idmod].child;
	ob.vCurr = 'ῶ';
	param.addEventListener(FocusEvent.FOCUS_OUT, f_FOCUS_OUT);
	param.addEventListener(FocusEvent.FOCUS_IN, f_FOCUS_IN);
	
	function f_FOCUS_IN():void
	{
		if(ob.vCurr != ''){
//			if (fplus == '')
				ob.vCurr = param.text;
			ob.vKeyOut=true;
		}
	}
	
	function f_FOCUS_OUT():void
	{
		if(ob.hasOwnProperty('vKeyOut'))
			if (ob.vKeyOut==true){
				if (ob.vCurr != param.text && ob.isUpdate == false)
					if (fplus==''){
						vfplus='';
						F_wsDelete('cekid',paket,ob.vUserId,param.text);
					}else
						F_wsDelete('cekid',paket,ob.vUserId, vfplus);
				ob.vKeyOut = false;
			}else ob.vKeyOut = true;
	}
}
public function F_cekKosong(arr:Array):Boolean{
	var i:int, ind:String='', ing:String='';
	for(i=0;i<arr.length;i++){
		if(arr[i].t.text == '' || arr[i].t.text == null || arr[i].t.text == undefined){
			ind += arr[i].n+', ';
			ing += arr[i].e+', ';
		}
	}
	if(ind == '' || ing == ''){
		return true;
	}else{
		Alert.show(ing.slice(0, ing.length-2)+' must be filled ('+ind.slice(0, ing.length-2)+' harus diisi dengan benar)');
		return false;
	}
}
public function F_configMasterInputHasil():void
{
	if(wsSuccess == true){
		switch(subject){
			case 'simpan':
				closeFormPopUp(1);
				if(wsSuccess == true){
					m[idmod].child.dg.dataProvider = wsArrUtama;
					m[idmod].child.dg.setSelectedCell(0,0);
					rowIndex = 0;
					m[idmod].child.txtpaging.text = wsCurPage;
				}else m[idmod].child.dg.dataProvider.removeAll();
				break;
			case 'simpan_baru':
				if(wsSuccess == true){
					m[idmodcurr].child.dg.dataProvider = wsArrUtama;
					m[idmodcurr].child.dg.setSelectedCell(0,0);
					rowIndex = 0;
					m[idmodcurr].child.txtpaging.text = wsCurPage;
				}else m[idmodcurr].child.dg.dataProvider.removeAll();
				ob.txtkode.enabled =true;
				ob.txtkode.setFocus();
				break;
			case 'cekid':ob.vCurr = 'ῶ';break;
		}
	}else{
		switch(subject){
			case 'simpan':
			case 'simpan_baru':Alert.show(wsErrmessage);break;
			case 'cekid' :
				ob.vCurr = '';
//				if (vfplus=='')
					ob.txtkode.setFocus();
					FlexGlobals.topLevelApplication.DefaultTanggalforDB
				Alert.show('Data sudah ada','Pesan Informasi');
				break;
		}
	}
}


public function F_MasterSimpan(baru:Boolean, paket:String, data:String, sort:String = ''):void
{
	switch(baru){
		case true://simpanbaru
			F_wsSimpan("simpan_baru", paket, ob.vUserId, ob.isUpdate, data, '',1,20,'',sort);
			ob.f_kondisi_awal();
			ob.isUpdate = false;
			break;
		case false: //simpan
			F_wsSimpan("simpan", paket, ob.vUserId, ob.isUpdate, data, '', 1, 20, '',sort);
			break;
	}
}

public function F_persiapanFormMaster():void
{
	var vSort:Boolean = true, vPosisi:int = 0, vCMI:ContextMenuItem, dg:DataGrid, mb:MenuBar, arr:Array = new Array;
	ob = m[idmod].child;
	dg = ob.dg; mb = ob.mb;
	ob.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	// Config Paging
	ob.txtpaging.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_pagingKEY_DOWN);
	ob.txtpaging.restrict = "0-9";
	ob.txtpaging.setStyle("textAlign","center");
	ob.txtpaging.text = 0;
	ob.txtpaging.maxChars = 4;
	ob.imgfirst.addEventListener(MouseEvent.CLICK, f_firstCLICK);
	ob.imgprevious.addEventListener(MouseEvent.CLICK, f_previousCLICK);
	ob.imgnext.addEventListener(MouseEvent.CLICK, f_nextCLICK);
	ob.imglast.addEventListener(MouseEvent.CLICK, f_lastCLICK);
	
	function f_KEY_DOWN(e:KeyboardEvent):void
	{ 
		switch(e.keyCode){
			case Keyboard.HOME:
				if(int(m[idmod].child.txtpaging.text)>1)
					f_formMasterTampil();
				break;
			case Keyboard.PAGE_UP:
				if(int(m[idmod].child.txtpaging.text)-1>0)
					f_formMasterTampil(int(m[idmod].child.txtpaging.text)-1);
				break;
			case Keyboard.PAGE_DOWN:
				f_formMasterTampil(int(m[idmod].child.txtpaging.text)+1);
				break;
			case Keyboard.END:
				f_formMasterTampil(-1);
				break;
			case Keyboard.F2:
				f_tambah();
				break;
			case Keyboard.E:
				if(e.ctrlKey)
					f_edit();
				break;
			case Keyboard.N:
				if(e.ctrlKey)
					f_tambah();
				break;
			case Keyboard.P:
				if(e.ctrlKey)
					f_cetak();
				break;
			case Keyboard.R:
				if(e.ctrlKey)
					f_reset();
				break;
		}
	}
	
	
	function f_firstCLICK():void
	{
		if(int(m[idmod].child.txtpaging.text)>1)
			f_formMasterTampil();
	}
	
	function f_previousCLICK():void
	{
		if(int(m[idmod].child.txtpaging.text)-1>0)
			f_formMasterTampil(int(m[idmod].child.txtpaging.text)-1);
	}
	
	function f_nextCLICK():void
	{
		f_formMasterTampil(int(m[idmod].child.txtpaging.text)+1);
	}
	
	function f_lastCLICK():void
	{
		f_formMasterTampil(int(-1));
	}
	
	function f_pagingKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.keyCode == Keyboard.ENTER)
			if(int(m[idmod].child.txtpaging.text)>0)
				f_formMasterTampil(int(m[idmod].child.txtpaging.text));
	}
	// Config Grid
	dg.dataProvider = new ArrayCollection();
	dg.addEventListener(GridSortEvent.SORT_CHANGING, f_dgSort_changing);
	dg.addEventListener(flash.events.MouseEvent.DOUBLE_CLICK, f_dgDOUBLE_CLICK);
	dg.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_dgKEY_DOWN);
	dg.addEventListener(GridEvent.GRID_ROLL_OVER, f_GRID_ROLL_OVER);
	dg.addEventListener(GridSelectionEvent.SELECTION_CHANGE, f_GRID_CHANGE);
	dg.selectionMode = "singleCell";
	dg.doubleClickEnabled = true;
	dg.contextMenu = new ContextMenu;
	dg.contextMenu.hideBuiltInItems();
	dg.contextMenu.addEventListener(ContextMenuEvent.MENU_SELECT, f_dgMENU_SELECT);
	vCMI = new ContextMenuItem('Edit')     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_edit);  
	dg.contextMenu.customItems.push(vCMI);    
	vCMI = new ContextMenuItem('Hapus')     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_hapus);  
	dg.contextMenu.customItems.push(vCMI);    
	function f_GRID_ROLL_OVER(e:GridEvent):void
	{
		vgMyRow = e.rowIndex;
		vgMyCol = e.columnIndex;
	}
	
	function  f_GRID_CHANGE(e:GridSelectionEvent):void{
		m[idmod].child.f_grid_change();
	}
	
	function f_dgMENU_SELECT():void
	{
		dg.setSelectedCell(vgMyRow, vgMyCol);
	}
	
	function f_dgKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.keyCode == Keyboard.DELETE)
			Alert.show(l('Hapus data ini ?'), l('Validasi'), 3, null, function(e:Object):void{	
				if(e.detail == Alert.YES)
					f_hapus()
			});
	}
	
	// Config Menu Bar
	mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, f_tampilkan); // tampil
	mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, f_reset); // reset
	mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, f_tambah); // tambah
	mb.menuBarItems[3].addEventListener(MouseEvent.CLICK, f_edit); // edit
	mb.menuBarItems[4].addEventListener(MouseEvent.CLICK, f_hapus); // hapus
//	mb.menuBarItems[5].addEventListener(MouseEvent.CLICK, f_cetak); // cetak
	mb.menuBarItems[6].addEventListener(MouseEvent.CLICK, f_catatan); // bantuan
	mb.menuBarItems[7].addEventListener(MouseEvent.CLICK, f_files); // bantuan
	mb.addEventListener(MenuEvent.ITEM_CLICK,function  klik(e:MenuEvent):void{
		ob.vfileextention=int(e.item.@data);
		ob.f_cetak();
	})
	function f_dgSort_changing(e:GridSortEvent):void
	{
		var om:Object = m[idmod].child.dg.columns.getItemAt(int(e.columnIndices)).dataField;
		if(vSort == true){ 
			vSort = false;
			f_formMasterTampil(1, String(om));
		}else if(vPosisi != int(e.columnIndices)){
			vSort = false;
			f_formMasterTampil(1, String(om));
		}else{
			vSort = true;dg.dataProvider.removeAll();
			f_formMasterTampil(1, om+' DESC');
		}
	}
	
	function f_dgDOUBLE_CLICK():void
	{
		f_edit();
	}
	
	function f_tampilkan():void
	{
		f_refreshIDmodule()
		if(int(m[idmod].child.txtpaging.text) == 0)
			f_formMasterTampil();
		else
			f_formMasterTampil(int(m[idmod].child.txtpaging.text));
	}
	
	function f_hapus():void
	{
		if(dg.dataProviderLength > 0)
			F_wsDelete(sptParam+'terkaitFormMaster', m[idmod].child.arr[6], userid, m[idmod].child.f_paramDelete(), 1, 20, m[idmod].child.f_filterdata()); 
	}
	
	function f_reset():void
	{
		ob.f_kondisiAwal();
	}
	
	function f_tambah():void
	{
		openFormX(l(m[idmod].child.arr[2]), 1, m[idmod].child.arr[3], m[idmod].child.arr[5], m[idmod].child.arr[4])
	}
	
	function f_edit():void
	{
		if(dg.dataProviderLength > 0)
			openFormX(l(m[idmod].child.arr[2]), 1, m[idmod].child.arr[3], m[idmod].child.arr[5], m[idmod].child.arr[4], 't1', '@')
	}
	
	function f_cetak():void
	{
		ob.f_cetak();
	}
	
	function  f_catatan():void
	{
		if(dg.selectedCell != null)
			F_OpenNotesMaster(m[idmod].child.arr[9][0], m[idmod].child.arr[9][1], m[idmod].child.arr[9][2], m[idmod].child.arr[9][3]);
	}
	
	function f_files():void
	{
		if(dg.selectedCell != null){
			F_OpenFilesMaster(m[idmod].child.arr[9][0], m[idmod].child.arr[9][1], m[idmod].child.arr[9][2], m[idmod].child.arr[9][3]);
		}
	}
	
	function f_bantuan():void
	{
		F_help(m[idmod].child.arr[5][0], m[idmod].child.arr[5][1]);
	}
	
}


protected var vMasterSort:String;
public function f_formMasterTampil(page:int = 1, sort:String = ''):void
{
	f_refreshIDmodule()
	if(sort == '')sort = m[idmod].child.arr[1];
	vMasterSort = sort;
	F_wsSearch("★tampilForm", m[idmod].child.arr[0], page, 20, m[idmod].child.f_filterdata(), sort);
}

public function F_dgRefresh(grid:DataGrid):void
{
	if(grid.dataProvider != null)
	(grid.dataProvider as ArrayCollection).refresh();
	mod('f_hitungTotal');
}

public function F_dgColumnRefresh(grid:DataGrid, row:int = -1):void
{
//	idmod = String(grid).split('.')[7];
	if(row == -1)row = rowIndex; 
	grid.dataProvider.setItemAt(grid.dataProvider.getItemAt(row), row);
	mod('f_hitungTotal');
}

public function F_acColumnRefresh(ac:ArrayCollection, row:int = -1):void
{
	if(row == -1)row = rowIndex; 
	ac.setItemAt(ac.getItemAt(row), row);
	mod('f_hitungTotal');
}

public function F_diskonBertingkat(jumlah:Number, harga:Number, diskon:String):Number
{
	var a:Array = diskon.split('+'), n:Number, i:int;
	n = jumlah*harga;
	if(a.length > 0){
		for(i=0;i<a.length;i++)
			n -= (a[i]/100)*n;
		n = (jumlah*harga)-n;
	}else if(int(diskon) == 0)n = 0;
	return n;
}

public function F_standartFilter():String
{
	var s:String, f:String, filter:String = '';
	ob = m[idmod].child
	f = ob.a[3];
	s = ob.cmbstatus.textInput.text;

	if(s == 'Semua' || s == ''){
		ob.cmbstatus.selectedIndex = 0;
		if(ob.a[4] == true)
			filter = TambahFilter(filter, "(case "+f+"inputuser WHEN "+userid+" THEN ("+f+"status = 0 OR "+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 5 OR "+f+"status = 6 OR "+f+"status = 7 OR "+f+"status = 8 OR "+f+"status = 9 OR "+f+"status = 10 OR "+f+"status = 11) else ("+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 5 OR "+f+"status = 6 OR "+f+"status = 7 OR "+f+"status = 8 OR "+f+"status = 9 OR "+f+"status = 10 OR "+f+"status = 11) end)");
		else
			filter = TambahFilter(filter, "(case "+f+"inputuser WHEN "+userid+" THEN ("+f+"status = 0 OR "+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 7) else ("+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 7) end)");
	}else if(s == 'Draft')
		filter = TambahFilter(filter, ""+f+"status = 0 AND "+f+"inputuser = "+userid);
	else
		filter = TambahFilter(filter, ""+f+"status = "+ob.cmbstatus.selectedItem.v);
	
	if(ob.txtnotransaksi1.text != '' && ob.txtnotransaksi2.text != '')
		filter = TambahFilter(filter, f+"notransaksi BETWEEN '"+ob.txtnotransaksi1.text+"' AND '"+ob.txtnotransaksi2.text+"'");
	
	if(ob.dttgl1.text != '' && ob.dttgl2.text != '')
		filter = TambahFilter(filter, f+"tgl BETWEEN '"+F_tglFormatDB(ob.dttgl1.text)+"' AND '"+F_tglFormatDB(ob.dttgl2.text)+"'"); 
	else if(ob.dttgl1.text != '') 
		filter = TambahFilter(filter, f+"tgl >= '"+F_tglFormatDB(ob.dttgl1.text)+"'"); 
	else if(ob.dttgl2.text != '')
		filter = TambahFilter(filter, f+"tgl <= '"+F_tglFormatDB(ob.dttgl2.text)+"'"); 
	
	if(ob.txtlokasi.text != '')
		filter = TambahFilter(filter, f+"lokasi LIKE '%"+ob.txtlokasi.text+"%'");
	
	if(ob.txturaian.text != '')
		filter = TambahFilter(filter, f+"uraian LIKE '%"+ob.txturaian.text+"%'");
	
	if(ob.txtcatatan.text != '')
		filter = TambahFilter(filter, f+"catatan LIKE '%"+ob.txtcatatan.text+"%'");
	
	if(ob.txtcabang.text != '')
		filter = TambahFilter(filter, f+"cabang LIKE '%"+ob.txtcabang.text+"%'");
	
	return filter;
}

public function F_hapusFormatCurrency(nilai:String):Number{
	return F_unCurreny(nilai);
}
public function F_filterEnter(txt:Array):void
{
	for (i=0;i<txt.length;i++){
		txt[i].addEventListener(KeyboardEvent.KEY_DOWN,function f_ENTER(e:KeyboardEvent):void{
			if (e.keyCode==Keyboard.ENTER){
				f_formMasterTampil(1,'')
			}
		});
	}
}

public function F_setProperty(components:String, dataField:String, data:Object, info:String = ''):void
{
	if(m.bukacompencarian)
		ob = winC;
	else{
		f_refreshIDmodule();
		ob = m[module].child;
	}
	ob.f_setProperty(components, dataField, data, info);
}

public function F_getProperty(components:String, dataField:String, data:Boolean, info:String = ''):Object
{
	switch(components){
		case 'checkBox':  
			ob.f_getProperty(dataField, data, info);
			break;
		case 'comboBox':  
			if(m[idmod].child.hasOwnProperty('f_getProperty'))
				return m[idmod].child.f_getProperty(components, dataField, data, info);
			break;
	}
	return null;
}

public function F_tglSelisih(date1:String, date2:String):int
{
	var d1:Array = date1.split('/'), d2:Array = date2.split('/');
	return getDaysBetweenDates(new Date(d1[2], d1[1], d1[0]), new Date(d2[2], d2[1], d2[0]));
	
	function getDaysBetweenDates(date1:Date,date2:Date):int
	{
		var oneDay:Number = 1000 * 60 * 60 * 24;
		var date1Milliseconds:Number = date1.getTime();
		var date2Milliseconds:Number = date2.getTime();         
		var differenceMilliseconds:Number = date1Milliseconds - date2Milliseconds;
		return Math.round(differenceMilliseconds/oneDay);
	}
}

public function F_getSetting(moduleID:int, menuID:int):String
{
	var arr:Array = arCoreData[11].split(sptRow), arr2:Array;
	for(i=0;i<arr.length;i++){
		arr2 = arr[i].split(sptField);
		if(arr2[0] == moduleID && arr2[1] == menuID){
			return arr2[2];
			break;
		}
	}
	return '';
}

public function F_jsonDecode(data:String):Object
{
	return com.adobe.serialization.json.JSON.decode(data);
}

public function F_jsonEn(data:String):Object
{
	return com.adobe.serialization.json.JSON.encode(data);
}

protected var loadJson:URLLoader;

public function F_jsonEncode(id:String, array:Object, namaKolom:Array):String
{
	var data:String, j:int;
	data = "{\"id\":\""+id+"\", \"property\":[";
	for(i=0;i<array.length;i++){
		if(i == 0){
			data += "{";
		}else{
			data += ", {";
		}
		for(j=0;j<namaKolom.length;j++){
			if(j == 0){
				data += "\""+namaKolom[j]+"\":\""+array[i][namaKolom[j]]+"\"";
			}else{
				data += ", \""+namaKolom[j]+"\":\""+array[i][namaKolom[j]]+"\"";
			}
		}
		data += "}"
	}
	data += "]}";
	return data;
}

public function F_jsonContent(id:String, content:String):String
{
	return "{\""+id+"\":["+content+"]}";
}


public function F_getDayCount(year:int, month:int):int{
	var d:Date=new Date(year, month, 0);
	return d.getDate();
}

public function F_tglJatuhTempo(date: String, value: Number):String
{
	var d1:Array = date.split('/')
	var tanggal : Date = new Date(d1[2], d1[1]-1, d1[0]);
	tanggal.setDate(tanggal.date + Number(value));
	var  CurrentDF:DateFormatter = new DateFormatter();
	CurrentDF.formatString = 'DD/MM/YYYY';
	return CurrentDF.format(tanggal);
}

public function F_defaultGrid(grid:Array):void
{
	var j:int;
	for(j=0;j<grid.length;j++){
		grid[j].dataProvider.removeAll();
		F_acAdd(grid[j]); 
	}
}

protected function f_toolTipeMySuperTab(index:int):String
{
	var s:String = MySuperTab.getItemAt(index).icon;
	s = s.split("[class MyERPPlus_").join("");
	return s.split("]").join("");
}
 
public function F_paramDate(date1:String, date2:String):String
{
	if(date1.length > 0 && date2.length > 0)
		return date1+" s.d "+date2;
	else if(date1.length > 0 && date2.length == 0)
		return date1+" s.d "+DefaultTanggal;
	else if(date1.length == 0 && date2.length > 0)
		return m.DateFirst+" s.d "+date2;
	else return m.DateFirst+" s.d "+DefaultTanggal;
}
// -------------------------------Akhir fungsi-fungsi -----------------------------------------