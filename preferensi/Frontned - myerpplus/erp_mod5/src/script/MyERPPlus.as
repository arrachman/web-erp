import MyClass.KelasKu;
import MyClass.StiReport;
import MyClass.myHardware;

import com.adobe.serialization.json.JSON;
import com.adobe.serializers.xml.XMLDecoder;

import comp.*;

import flash.display.Bitmap;
import flash.display.DisplayObject;
import flash.display.InteractiveObject;
import flash.events.ContextMenuEvent;
import flash.events.ErrorEvent;
import flash.events.Event;
import flash.events.FocusEvent;
import flash.events.IOErrorEvent;
import flash.events.KeyboardEvent;
import flash.events.MouseEvent;
import flash.events.ProgressEvent;
import flash.net.URLLoader;
import flash.net.URLRequest;
import flash.system.Security;
import flash.system.System;
import flash.ui.ContextMenu;
import flash.ui.ContextMenuItem;
import flash.ui.Keyboard;
import flash.utils.ByteArray;

import flashx.textLayout.elements.BreakElement;

import flexlib.controls.tabBarClasses.SuperTab;
import flexlib.events.SuperTabEvent;

import itemEditor.*;

import itemRenderer.*;

import mod.m0.m0_JurnalTerkait;
import mod.m0.m0_Login;
import mod.m0.m0_Picture;

import mx.collections.ArrayCollection;
import mx.collections.ArrayList;
import mx.containers.TabNavigator;
import mx.controls.Alert;
import mx.controls.Menu;
import mx.controls.MenuBar;
import mx.controls.ProgressBar;
import mx.controls.SWFLoader;
import mx.controls.TextInput;
import mx.core.ClassFactory;
import mx.core.FlexGlobals;
import mx.core.FlexLoader;
import mx.core.IFactory;
import mx.core.UIComponent;
import mx.events.CloseEvent;
import mx.events.DividerEvent;
import mx.events.FlexEvent;
import mx.events.IndexChangedEvent;
import mx.events.ListEvent;
import mx.events.MenuEvent;
import mx.events.ResizeEvent;
import mx.formatters.DateFormatter;
import mx.managers.CursorManager;
import mx.managers.FocusManager;
import mx.managers.PopUpManager;
import mx.messaging.AbstractConsumer;
import mx.messaging.messages.ErrorMessage;
import mx.rpc.Fault;
import mx.rpc.events.FaultEvent;
import mx.rpc.events.InvokeEvent;
import mx.rpc.events.ResultEvent;
import mx.rpc.soap.LoadEvent;
import mx.rpc.soap.mxml.Operation;
import mx.rpc.soap.mxml.WebService;
import mx.utils.ObjectUtil;

import net.user1.reactor.System;

import org.flexunit.runners.ParentRunner;

import script.*;

import skins.dgStyle;

import spark.components.*;
import spark.components.gridClasses.GridColumn;
import spark.events.GridEvent;
import spark.events.GridItemEditorEvent;
import spark.events.GridSelectionEvent;
import spark.events.GridSortEvent;
import spark.events.IndexChangeEvent;
import spark.events.TextOperationEvent;
import spark.formatters.NumberFormatter;
import spark.modules.ModuleLoader;

[Bindable] [Embed(source="assets/main/doubleright.gif")]  public var tbdoubleright:Class; //Double Right
[Bindable] [Embed(source="assets/main/downarrow.png")]  public var tbdownarrow:Class; //Down Arrow
[Bindable] [Embed(source="assets/main/user.png")]  public var mainuser:Class;
[Bindable] [Embed(source="assets/main/fullscreen.png")]  public var mainfullscreen:Class;
[Bindable] [Embed(source="assets/main/header.png")]  public var pheader:Class; // Parent Header
[Bindable] [Embed(source="assets/main/left.png")]  public var mainleft:Class;
[Bindable] [Embed(source="assets/main/folderopen.png")]  public var folderopen:Class;
[Bindable] [Embed(source="assets/main/folderclose.png")]  public var folderclose:Class;
[Bindable] [Embed(source="assets/main/right.png")]  public var pright:Class; // Parent Right
[Bindable] [Embed(source="assets/main/t0.png")]  public var t0:Class; //Master Data
[Bindable] [Embed(source="assets/main/t1.png")]  public var t1:Class; //Master Data
[Bindable] [Embed(source="assets/main/t2.png")]  public var t2:Class; //Transaksi
[Bindable] [Embed(source="assets/main/t3.png")]  public var t3:Class; //Data
[Bindable] [Embed(source="assets/main/t4.png")]  public var t4:Class; //Laporan
[Bindable] [Embed(source="assets/main/t5.png")]  public var t5:Class; //Setting
[Bindable] [Embed(source="assets/main/t6.png")]  public var t6:Class; //Chart
[Bindable] [Embed(source="assets/main/t7.png")]  public var t7:Class; //KPI
[Bindable] [Embed(source="assets/main/logout2.png")]  public var mainlogout:Class;
[Bindable] [Embed(source="assets/main/language.png")]  public var tblanguage:Class; //Select
[Bindable] [Embed(source="assets/main/calendar2.png")]  public var maindate:Class;
[Bindable] [Embed(source="assets/mod/m0.png")]  public var iconm0:Class; //Administrator
[Bindable] [Embed(source="assets/mod/m1.png")]  public var iconm1:Class; //Master Data
[Bindable] [Embed(source="assets/mod/m2.png")]  public var iconm2:Class; //Finance & Accounting
[Bindable] [Embed(source="assets/mod/m3.png")]  public var iconm3:Class; //Inventory & Warehouse
[Bindable] [Embed(source="assets/mod/m4.png")]  public var iconm4:Class; //Purchasing
[Bindable] [Embed(source="assets/mod/m5.png")]  public var iconm5:Class; //Sales
[Bindable] [Embed(source="assets/mod/m6.png")]  public var iconm6:Class; //Production
[Bindable] [Embed(source="assets/mod/m7.png")]  public var iconm7:Class; //Fixed Asset
[Bindable] [Embed(source="assets/mod/m8.png")]  public var iconm8:Class; //HRD & Payroll
[Bindable] [Embed(source="assets/mod/m9.png")]  public var iconm9:Class; //HRD & Payroll
[Bindable] [Embed(source="assets/mod/m11.png")]  public var iconm11:Class; //HRD & Payroll
[Bindable] [Embed(source="assets/mod/m12.png")]  public var iconm12:Class; //HRD & Payroll
[Bindable] [Embed(source="assets/mod/m_default.png")]  public var iconm_default:Class; //HRD & Payroll
[Bindable] [Embed(source="assets/tb/find.png")]  public var tbfind:Class; //Find

// Variable Profile User
public var userid:int, ukode:String, unama:String, upassword:String, ukontak:int, ukontakkode:String, 
	ukontaknama:String, ucabang:String, ukota:String,ugambar:String, uaktif:int,
	ucabangnama:String, ulokasi:String, ulokasinama:String, ugudang:String, ugudangnama:String, 
	utglexpired:String, ulevel:int, ugrup:int, ugrupnama:String, ubahasa:String, udefaultview:String, 
	ubahasanama:String, ukotanama:String, kontakperson:String, kontakpersonalamat:String,
	udefaultip:String;

// Variable diakses anaknya
public var FormFilter:String = '', kelasku:KelasKu = new KelasKu;
public var FfilterKontak:String = '';

// Variable Global Object
public var m:Object = {}, o:Object, ob:Object;

// Variable Perulangan
protected var i:int, j:int;

// Variable Split Result Web Service
public var sptParam:String, sptSubParam:String, sptRow:String, sptField:String, sptLogin:String;

// Variable Set Result Web Service
public var ob_resultWs:String, vArrResult:Array, vResult:Array, vPaging:Array, vData:Array, 
	wsTarget:String, wsSuccess:Boolean, wsErrmessage:String, wsErrstep:String, wsIdtransaksi:String,
	wsIspaging:Boolean, wsIsNext:Boolean, wsIsPrev:Boolean, wsCurPage:int, wsCountRow:int,
	wsArrUtama:ArrayCollection, wsArrDetail:ArrayCollection, vWsResult:ArrayCollection, 
	vNamaKolom:Array, wsJson:Object, arCoreData:Array;

// Variable komoponen pencarian
public var vpIdTxt:Object, vpFocusEvent:Boolean = true, vpSetIn:Boolean = true, 
	vpOnEnter:Boolean = false, vpVarCurrent:String = '', vpFocusEnter:Boolean = false;

// Variable TreeKu list module
public var arrModule:ArrayCollection;

// Variable Komponen-Komponen
public var winM:ComSearchMenu; // Komponen Cari menu cepat
public var winC:ComPencarian; // Komponen Pencarian
public var winRelogin:ComReLogin; // Komponen Login ulang
public var winLang:ComLanguage = new ComLanguage(); // Komponen ganti bahasa langsung
public var winCal:ComCalculator= new ComCalculator();  // Komponen kalkulator praktis
public var winCam:ComWebCamera = new ComWebCamera();  // Komponen Web Camera Canggih
public var winImp:ComImportData = new ComImportData();  // Komponen Import Data
public var winAlertCetak:ComAlertCetak = new ComAlertCetak(); // Komponen ALert Cetak
public var sumber:String = "";
public var winHardware:myHardware;
public var tmrHardware:Timer = new Timer(5000, 60*60*24);
// Variable Default Tanggal
public var DefaultTanggal:String, DefaultTanggalforDB:String, 
	DefaultTanggalforDBSlash:String, subject:String;

// Variable dll
public var WebAccessKey:String = 'Komodo Software', ExpandMenuPertama:Boolean, bHxbottom:Boolean = false,
	currentModuleID:int, currentMenuID:int, idmod:String="", module:String, wsFocus:Boolean = true, kategoriPOS:String = "", txtGudang:String = "";

public var vAlert:Alert;

// initialize untuk mendapatkan halaman login
protected function f_initializeHandler():void
{
	m.objTemp = {};
	// Modul Login
//	m.MlLogin = new m0_Login;
//	m.MlLogin.id = 'MlLogin';
//	m.MlLogin.x = 0;
//	m.MlLogin.y = 0;
//	m.MlLogin.percentWidth = 100;
//	m.MlLogin.percentHeight = 100;
//	addElement(m.MlLogin);
}

// checking file dan download file app.xml, grid.json, report.json dan statistic.json
protected function f_checkingFile():void
{
	try
	{
		m.pertama = true;
		// perintah untuk mengakses dari js, ini akses fungsi logout yg ada di index.html
		ExternalInterface.addCallback("aksesfromjs", aksesfromjs);
		
		// keamanan, ijinkan semua domain yg mau di akses
		Security.allowDomain("*");
		Security.allowInsecureDomain("*");
		Security.loadPolicyFile("xmlsocket://192.168.0.36:4444");
		// Set Default Jumlah logout 0
		m.jmllogout = 0;
		
		// Set default group busy visible try
		grpBusy.visible = true;
		
		// Set default Group Busy aplga 1
		grpGEBusy.alpha = 1;
		
		// Buat label Checking File
		m.o = new spark.components.Label;
		m.o.height = 76;
		m.o.horizontalCenter = 0;
		m.o.verticalCenter = 0;
		m.o.setStyle('fontSize', '18');
		m.o.setStyle('fontWeight', 'bold');
		m.o.text = "Downloading File";
		addElement(m.o);
		
		// Buat label Checking File 'Please Wait'
		m.l = new spark.components.Label;
		m.l.height = 76;
		m.l.horizontalCenter = 0;
		m.l.verticalCenter = 25;
		m.l.setStyle('fontSize', '18');
		m.l.setStyle('fontWeight', 'bold');
		m.l.text = "Please Wait";
		addElement(m.l);
		
		// Set default sort global
		m.sort = {};
		
		// Set default mod global
		m.mod = {}; 
		
		//m.o.text = "Downloading 0 of 3 : Setting Configurations";
		m.o.text = "App Configuration";
		
		// Setting Checking File Loader
		m.lj = new URLLoader;
		m.lj.addEventListener(Event.COMPLETE, onload);
		m.lj.addEventListener(IOErrorEvent.IO_ERROR, onerror);
		m.lj.load(new URLRequest("app.xml")); // Download file app.xml

		// Function Onload ketika COMPLETE 'CheckFile Loader'
		function onload(e:Event):void
		{
			try
			{
				// Set Setting app
				m.app = new XML(e.target.data);
				
				// Set url
				m.url = m.app.url;
				
				// Setting Icon Logo
				imgLogo.source = m.url+"/app/css/Logo ERP/Logo_parent.png";
				imgLogo.validateNow();
				
				// Set kode app
				m.kodeapp = m.app.kodeapp;

				// Set Title Company
				ExternalInterface.call('f_setTitle', String(m.app.namapt));
				
				// Ganti status CheckingFile
				m.o.text = "Downloading 0 of 3 : Web Service Configurations";
				
				// Buat Web Service
				m.myWs = new WebService;
				m.myWs.addEventListener(ResultEvent.RESULT, F_wsOperation_result);
				m.myWs.addEventListener(FaultEvent.FAULT, F_wsOperation_fault);
				m.myWs.addEventListener(LoadEvent.LOAD, F_wsOperation_load);
				m.myWs.addEventListener(InvokeEvent.INVOKE, F_wsOperation_invoke);
				m.myWs.wsdl = m.url+"/ws/myerpplus.asmx?wsdl";
				m.myWs.showBusyCursor = true;
				m.myWs.loadWSDL();
					
			} 
			catch(error:Error) 
			{ 
				Alert.show(error.getStackTrace(), l('Informasi'));
			}
			
			// Fungsi result myWs
			function F_wsOperation_result(event:ResultEvent):void
			{
				try
				{
					// Ketika variable wsUploadData ada isinya, maka di set undefined.
					if(m.wsUploadData != undefined)
						m.wsUploadData = undefined;
					
					// Set Result Web Service
					ob_resultWs = String(event.result);
					
					// Ketika Result WebService "Invalid Website Access Key" maka keluar form Relogin
					if(ob_resultWs == "Invalid Website Access Key."){
						wsValidasi = false;
						f_getReloginForm();
						return;
					}
					
					// Set Result Web Service
					f_setVariableWS();
					
					// Get Fungsi Result
					f_result();
				} 
				catch(error:Error) 
				{
					Alert.show(error.getStackTrace(), l('Informasi'));
				}
				
			}
			
			// Fungsi fault myWs
			function F_wsOperation_fault(event:FaultEvent):void
			{
				try
				{
					// Set okLabel jadi Reload
					Alert.okLabel = l('Load Ulang');
					
					// Muncul Konfirmasi Alert Saat Web Service mengalami error atau gangguan
					vAlert = Alert.show(l('MyERP Plus tidak terkoneksi dengan server !'), l('Konfirmasi'), 4, null, wsfault);
					
					// Set default okLabel jadi Ok
					Alert.okLabel = 'Ok';
					
					f_refreshIDmodule();
					mod("wsFault");
				} 
				catch(error:Error) 
				{
					Alert.show(error.getStackTrace(), l('Informasi'));
				}
				
				function wsfault():void{	
					// Ketika wsParam terisi
					if(wsParam.length > 0)
					{
						// Ketika wsUploadData undefined
						if(m.wsUploadData == undefined)
							
							// Jalankan Web Serive lagi
							m.myWs.Ws(wsParam);
						else
							
							// Jalankan Web Service Upload lagi
							m.myWs.UploadFile(wsParam, m.wsUploadData);
					}else
						// Ketika wsParam kosong maka buka lagi Halaman ini 'index.html'
						navigateToURL(new URLRequest(m.url+""), "_self");
				}
			}
			
			// Fungsi load myWs
			function F_wsOperation_load(event:LoadEvent):void
			{
				try
				{
					// Set variable Split param standart
					sptParam = "★";
					sptSubParam = "△";
					sptRow = "▲";
					sptField = "▼";
					sptLogin = "Θ";
					
					m.mulai = false;
					
					// Ganti status CheckingFile
					m.o.text = "Downloading 1 of 3 : Grid Configurations";
					
					F_wsSearch(sptParam+"M0_GetLibraryGrid", "M0_GetLibraryGrid");
				} 
				catch(error:Error) 
				{
					Alert.show(error.getStackTrace(), l('Informasi'));
				}
			}
			
			// Fungsi invoke myWs
			function F_wsOperation_invoke(event:InvokeEvent):void
			{
				try
				{
					if(m.mulai)
					{
						m.tmrBusy.reset();
						m.tmrBusy.start();
						m.objFocus = focusManager.getFocus();
						if(wsFocus == true && m.objFocus != null)
							grpBusy.setFocus();
						else m.objFocus != null
					}
				} 
				catch(error:Error) 
				{
					Alert.show(error.getStackTrace(), l('Informasi'));
				}
			}
		}
		
		// Function onerror ketika IO_ERROR 'CheckFile Loader'
		function onerror(e:IOErrorEvent):void
		{
			try
			{
				Alert.show("File app.xml "+l("tidak ada"), l("Informasi"));
			} 
			catch(error:Error) 
			{
				Alert.show(error.getStackTrace(), l('Informasi'));
			}
		}
		// -------- Setting TreeKu
		TreeKu.useHandCursor = true;
		TreeKu.buttonMode = true;
		TreeKu.labelField = "@l";
		TreeKu.iconField = "@t";
		TreeKu.showRoot = false;
		TreeKu.setStyle("indentation", 5);
		TreeKu.setStyle("folderOpenIcon", folderopen);
		TreeKu.setStyle("folderClosedIcon", folderclose);
		TreeKu.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_treeKeyDown);
		TreeKu.addEventListener(mx.events.ListEvent.ITEM_CLICK, f_treeClick);
		TreeKu.addEventListener(mx.events.FlexEvent.UPDATE_COMPLETE, ExpandFavorit);
		
		// Fungsi ExpanFavorit ketika Update Complete 'Treeku'
		function ExpandFavorit():void {
			try
			{
				if(ExpandMenuPertama == true){
					ExpandMenuPertama=false;
					TreeKu.selectedIndex  = 0;
					m.selectedNode=TreeKu.selectedItem as XML;
					TreeKu.expandItem(m.selectedNode,true,false);
				}
			} 
			catch(error:Error) 
			{
				//tampilkan error message
				Alert.show(error.getStackTrace(), l("Informasi"));
			}
		}
		// -------- End Setting TreeKu
		
		// Setting Label User Name
		LblUsername.setStyle("fontFamily", "Arial");
		LblUsername.setStyle("fontSize", 14);
		LblUsername.setStyle("textAlign", "right");
		
		// Setting icon User
		imguser.buttonMode = true;
		imguser.useHandCursor = true;
		imguser.source = mainuser;
		imguser.toolTip = "Change Profil User";
		imguser.addEventListener(MouseEvent.CLICK, function():void{f_iconMenuHandler("UserProfil")});
		
		// Setting icon Tanggal
		imgtgl.buttonMode = true;
		imgtgl.useHandCursor = true;
		imgtgl.source = maindate;
		imgtgl.toolTip = "Change Default Date";
		imgtgl.addEventListener(MouseEvent.CLICK, function():void{f_iconMenuHandler("Tanggal")});
		
		// Setting icon Full Screen
		imgscreen.buttonMode = true;
		imgscreen.useHandCursor = true;
		imgscreen.source = mainfullscreen;
		imgscreen.toolTip = "Full Screen";
		imgscreen.addEventListener(MouseEvent.CLICK, imgscreen_clickHandler);
		
		// Setting icon Full Screen
		imglogout.buttonMode = true;
		imglogout.useHandCursor = true;
		imglogout.source = mainlogout;
		imglogout.toolTip = "Log Out";
		imglogout.addEventListener(MouseEvent.CLICK, function():void{f_iconMenuHandler("Log Out")});
		
		// Setting Box Content
		hDiv.liveDragging = true;
		hDiv.addEventListener(mx.events.DividerEvent.DIVIDER_DRAG, hdividedbox1_dividerDragHandler);
		
		// Setting Button Link Judul
		BtnLinkJudul.setStyle("fontWeight", "bold");
		BtnLinkJudul.setStyle("textAlign", "left");
		
		// Setting Button Collapse
		BtnCollapse.styleName = "SoftBlueCircle";
		BtnCollapse.addEventListener(MouseEvent.CLICK, BtnCollapse_clickHandler);

		// Setting Box Grup Menu
		BoxGrupMenu.setStyle("verticalGap", 0);
		BoxGrupMenu.addEventListener(mx.events.ResizeEvent.RESIZE, vx_resizeHandler);
		
		// Setting Button Module
		vx.setStyle("verticalGap", 0);
		vx.setStyle("horizontalAlign", "left");
		vx.verticalScrollPolicy = "off";
		
		// Setting MySuperTab
		MySuperTab.setStyle("paddingTop", 0);
		MySuperTab.closePolicy = SuperTab.CLOSE_ROLLOVER;
		MySuperTab.addEventListener(mx.events.IndexChangedEvent.CHANGE, MySuperTab_changeHandler);
		MySuperTab.addEventListener(mx.events.IndexChangedEvent.CHILD_INDEX_CHANGE, MySuperTab_childIndexChangeHandler);
		
		// Setting Line Bottom
		linebottom.scaleMode = "stretch";
		linebottom.smooth = true;
		linebottom.source = pheader;
		linebottom.setStyle("smoothingQuality", "high");
		 
		// Setting Button Expand
		BtnExpand.styleName = "SoftBlueCircle";
		BtnExpand.visible = false;
		BtnExpand.addEventListener(MouseEvent.CLICK, BtnExpand_clickHandler);
		
		// Setting Grup Busy
		grpBusy.addEventListener(MouseEvent.CLICK, grpBusy_clickHandler);
		
	} 
	catch(e:Error){f_error('f_checkingFile', e)}
}

// persiapan dan setting komponen-komponen induk
protected function f_persiapanInduk():void
{
	try
	{
		// Membuat Dynamic Loader Sebanyak 21 Modul
		for(i=0 ; i<=20 ; i++){
			m['DynamicLoader'+i] = new ComModuleLoader;
			m['DynamicLoader'+i].id = 'DynamicLoader'+i;
			m['DynamicLoader'+i].url = '';
		}
		
		// Membuat Window Popup Sebanyak 4 Popup
		for(i=0;i<=5;i++){
			m['winPopUp'+i] = new TitleWindow;
			m['winPopUp'+i].id = 'winPopUp'+i;
			add(i);
			
			m['popUpLoader'+i] = new ComModuleLoader;
			m['popUpLoader'+i].id = 'popUpLoader'+i;
			
			m['winPopUp'+i].addElement(m['popUpLoader'+i]);
		}
		
		function add(i:int):void
		{
			m['winPopUp'+i].addEventListener(CloseEvent.CLOSE, function():void{closeFormPopUp(i)});
		}
		
		m.fullscreen = false;
		
		// Klik kanan parent
		contextMenu = new ContextMenu;
		contextMenu.hideBuiltInItems();
		m.cm = new ContextMenuItem("Close This");
		m.cm.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_closeThis);
		contextMenu.customItems.push(m.cm);
		m.cm = new ContextMenuItem("Close All");
		m.cm.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_closeAll);
		contextMenu.customItems.push(m.cm);
		m.cm = new ContextMenuItem("Close Others");
		m.cm.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_closeOthers);
		contextMenu.customItems.push(m.cm);
		m.cm = new ContextMenuItem("Reload");
		m.cm.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_reload);
		contextMenu.customItems.push(m.cm);
		
		function f_closeThis():void
		{	
			f_tabCloseActive();
		}
		
		function f_closeAll():void
		{	
			f_tabCloseAll();
		}
		
		function f_closeOthers():void
		{
			for(i=MySuperTab.length-1;i>=0;i--)
				if(MySuperTab.getChildAt(MySuperTab.selectedIndex)['label'] != MySuperTab.getChildAt(i)['label'] && MySuperTab.getChildAt(i)['label'] != "Home"){
					closeForm(MySuperTab.getChildAt(i)['label'], m.ArrTab[CariIndexTab(MySuperTab.getChildAt(i)['label'], f_toolTipeMySuperTab(i))].loader);
					MySuperTab.removeChildAt(i);
				}
		} 
		
		function f_reload():void
		{
			var data:Object = m.mod[idmod];
			FormFilter = "";
			m.ArrFilter = [];
			with(m[idmod]){
				x = 0;
				y = 0; 
				percentHeight=100;
				percentWidth=100;	
				unloadModule();
				loadModule();
			}
//			f_tabCloseActive();
//			openFormX(data.StrLabel, data.IsPopup, data.StrURL, data.Tinggi, data.Lebar, data.Tipe, data.FormFilter, data.ArrFilter, 1, MySuperTab.selectedIndex-1);
		} 
		
		// Register Busy Timer
		m.tmrBusy = new Timer(500, 10);
		m.tmrBusy.addEventListener(TimerEvent.TIMER, f_tmrBusy);	
		
		function f_tmrBusy(event:TimerEvent):void{
			grpBusy.visible = true;
			if(event.target.currentCount == '5')grpGEBusy.alpha = 0.25;
			if(event.target.currentCount == '10')grpGEBusy.alpha = 0.5;
		}
		
		// Get File Json
		loadJson = new URLLoader;
		loadJson.addEventListener(Event.COMPLETE, onload);
		loadJson.addEventListener(IOErrorEvent.IO_ERROR, onerror);
		
		function onload(e:Event):void
		{
			wsSuccess = true;
			if(e.target.data != undefined)
				wsJson = F_jsonDecode(e.target.data);
			f_result();
		}
		
		function onerror(e:IOErrorEvent):void
		{
			wsSuccess = false;
			f_result();
		}
		
		// Inisialisasi Parent
		btnbottom.setStyle('icontop', tbdoubleright);
		btnbottom.setStyle('iconbottom', tbdownarrow);
		btnbottom.styleName = 'parent';
		
		// Set Array dari database (Usermenu, setting, user menu custom, user menu report, nomor, periode akuntasi, Report, tree expand, tab, tree item, )
		m.ArrUserMenu = new ArrayCollection;
		m.ArrSetting = new ArrayCollection;
		m.ArrSettingPOS = new ArrayCollection;
		m.ArrPos_Bonus_ItemSearch = new ArrayCollection;
		m.ArrPos_Bonus_Item_DetailSetting = new ArrayCollection;
		m.ArrPos_Substitution_ItemSearch = new ArrayCollection;
		m.ArrPos_Substitution_Item_DetailSetting = new ArrayCollection;
		m.ArrPos_Additional_ItemSearch = new ArrayCollection;
		m.ArrPos_Additional_Item_DetailSetting = new ArrayCollection;
		m.ArrPos_Discount_ItemSearch = new ArrayCollection;
		m.ArrPos_Discount_Category_ItemSearch = new ArrayCollection;
		m.ArrPos_Point_ItemSearch = new ArrayCollection;
		m.ArrPos_Point_Category_ItemSearch = new ArrayCollection;
		m.ArrPos_Point_TransactionSearch = new ArrayCollection;
		m.ArrPos_Bonus_TransSearch = new ArrayCollection;
		m.ArrPos_Bonus_Trans_DetailSetting = new ArrayCollection;
		m.ArrUserMenuCustom = new ArrayCollection;
		m.ArrUserMenuReport = new ArrayCollection;
		m.ArrNomor = new ArrayCollection;
		m.ArrPeriodeAkuntansi = new ArrayCollection;
		m.ArrReport = new ArrayCollection;
		m.ArrTreeExpand = new ArrayCollection;
		m.ArrTab = new ArrayCollection;
		m.ArrTreeItem = [];
		m.ArrFilter = [];
		m.focusComboBox = true;
		m.bukacompencarian = false;
		m.bukacompencarian2 = false;
		
		// Set Default Tanggal Untuk Database "YYYY-MM-DD"
		DefaultTanggalforDB = CurrentDateTimeString("YYYY-MM-DD");
		
		// Set Default Tanggal Untuk Dabase yg slash "YYYY/MM/DD"
		DefaultTanggalforDBSlash = CurrentDateTimeString("YYYY/MM/DD");
		
		// Default Button Expand false
		BtnExpand.visible=false;
		
		// Default Variable Reset false
		m.objTemp.reset = false;
		
		// Buat Baru Komponen Pencarian
		winC = new ComPencarian();
		 
		// set variable array modTemp kosong
		m.modTemp = [];
		
		// Modul Login
		m.MlLogin = new m0_Login;
		m.MlLogin.id = 'MlLogin';
		m.MlLogin.x = 0;
		m.MlLogin.y = 0;
		m.MlLogin.percentWidth = 100;
		m.MlLogin.percentHeight = 100;
		addElement(m.MlLogin);
		
		// Hapus label progres dan please wait
		removeElement(m.o);
		removeElement(m.l);
	} 
	catch(e:Error){f_error('f_persiapanInduk', e)}
}

public function f_error(fungsi:String, e:Object):void
{
	Alert.show(fungsi+" : "+e.getStackTrace(), l('Informasi'));
}

protected function f_kondisiAwal():void
{		
	try
	{
		//tambahkan komponen pencarian
		
		// Set User Profile
		ob = wsArrUtama[0];
		userid = ob.f0;
		ukode = ob.f1;
		unama = ob.f2;
		upassword = ob.f3;
		ukontak = ob.f4;
		ukontakkode = ob.f5;
		ukontaknama = ob.f6;
		ucabang = ob.f7;
		ucabangnama = ob.f8; 
		ulokasi = ob.f9;
		ulokasinama = ob.f10;
		ugudang = ob.f11;	
		ugudangnama = ob.f12;
		ukota = ob.f13;
		ugambar = ob.f14;
		uaktif = ob.f15;
		utglexpired = ob.f16;
		ulevel = ob.f17;
		ugrup = ob.f18;
		ugrupnama = ob.f19;
		ubahasa = ob.f20;
		udefaultview = ob.f21;
		ubahasanama = ob.f22;
		ukotanama = ob.f23;
		kontakperson = ob.f24;
		kontakpersonalamat = ob.f25;
		udefaultip = ob.f26;
		LblUsername.text = unama;
		
		// split ws login
		arCoreData = vArrResult[2].split(sptLogin);
		
		// Create Menu Module
		m.wsData = f_ArrDetail(arCoreData[1]);
		
		// ------- Set Menu kiri (modul-modul)
		m.modid = m.wsData[0].f0;
		BtnLinkJudul.label = m.wsData[0].f2;
		
		// variable btn set default 
		m.btn = [];
		
		// HX Bottom  hapus semua anak 
		hxbottom.removeAllChildren();
		
		// HX Bottom buat button bottom
		hxbottom.addChild(btnbottom);
		
		// bHxbottom di set false;
		bHxbottom = false;
		
		// arrModule di set new ArrayCollection
		arrModule = new ArrayCollection;
		
		// vx Remove semua anak
		vx.removeAllChildren();
		
		// Perulangan untuk membuat button modul baru
		for(i=0;i<m.wsData.length;i++)
		{
			// setting button modul
			m.btn[i] = new Button();
			m.btn[i].label = m.wsData[i].f2;
			m.btn[i].id = "mn" + m.wsData[i].f0;
			m.btn[i].height = 30;
			m.btn[i].percentWidth = 100;
			m.btn[i].buttonMode = true; 
			m.btn[i].useHandCursor = true;
			m.btn[i].styleName = "parent";
			try
			{
				m.btn[i].setStyle("icon", this["iconm"+m.wsData[i].f0]);
			} 
			catch(error:Error) 
			{
				m.btn[i].setStyle("icon", this["iconm_default"]);
			}
			m.btn[i].addEventListener(MouseEvent.CLICK, f_btnModuleClick);
			
			//arrModule Add Data module (ID dan Nama)
			arrModule.addItem({mid:m.wsData[i].f0, mname:m.wsData[i].f2});
			
			// jika modul administrator, master data, Production, Fixed asset, HRD & Payroll maka button di kecilkan
//			if(m.btn[i].id == "mn"+0 || m.btn[i].id == "mn"+1 || m.btn[i].id == "mn"+6 || m.btn[i].id == "mn"+7 || m.btn[i].id == "mn"+8){
			if(i>3){	 
				m.btn[i].height = 28;
				m.btn[i].width = 28;
				hxbottom.addChild(m.btn[i]); 
				if(bHxbottom == false){
					bHxbottom = true;
					hxbottom.removeChild(btnbottom);
				}
			}else
				vx.addChild(m.btn[i]);
			
		} 
		vx.height = m.wsData.length * 30;
		with(BoxGrupMenu){
			height = vx.height+28;
			maxHeight = height;
			if(vx.numChildren >= 4)
				minHeight = (4 * 30) + 28;	
			else
				minHeight = ((vx.numChildren-1) * 30) + 28;	
		}
		if(bHxbottom)
			BoxGrupMenu.height = (BoxGrupMenu.minHeight+m.jmllogout);

		//fungsi untuk menu kiri modul-modul di klik 
		function f_btnModuleClick(event:Event):void {
			m.s = event.currentTarget.id.split("mn").join("");
			currentModuleID=int(m.s);
			TreeKu.dataProvider = XML(m.ArrTreeItem[m.s]);
			BtnLinkJudul.label = event.currentTarget.label;
			ExpandMenuPertama=true;
		}
		// ------- END Set Menu kiri (modul-modul)
		
		// Set Tree Item
		m.ArrTreeItem = arCoreData[2].split("|");
		TreeKu.dataProvider = XML(m.ArrTreeItem[m.modid]);
		ExpandMenuPertama = true;
		currentModuleID = m.modid;
		
		// Set Setting
		m.ArrSetting.removeAll();
		m.wsData = f_ArrDetail(arCoreData[3]);
		for(i=0;i<m.wsData.length;i++)
		{
			if(m.wsData[i].f1 == "company" && m.wsData[i].f2 == "HostPort")
			{
				m.arr = m.wsData[i].f6.split("|");
				FlexGlobals.topLevelApplication.m.defaultHost = m.arr[0];
				FlexGlobals.topLevelApplication.m.defaultPort = m.arr[1];
			}
			m.ArrSetting.addItem({smodule:m.wsData[i].f0, sgrup:m.wsData[i].f1, skode:m.wsData[i].f2, snilai:m.wsData[i].f6});
		}
		
		// Set Default Nominal dan Number
		m.formatMinusApp = getSetting(0, 'company', 'FormatMinusApp');
		m.formatNominal = getSetting(0, 'company', 'FormatNominal').split('|');
		
		// Set Formatter Number, ex: 1000
		m.nfNumber = new NumberFormatter;
		m.nfNumber.groupingSeparator = m.formatNominal[0];
		m.nfNumber.decimalSeparator = "";
		m.nfNumber.fractionalDigits = "0";
		m.nfNumber.negativeNumberFormat = m.formatMinusApp;
		
		// Set Formatter Nominal, ex: 1.000,00
		m.nfNominal = new NumberFormatter;
		m.nfNominal.groupingSeparator = m.formatNominal[0];
		m.nfNominal.decimalSeparator = m.formatNominal[1];
		m.nfNominal.fractionalDigits = m.formatNominal[2];
		m.nfNominal.negativeNumberFormat = m.formatMinusApp;
		
		// Set Company Profile
		m.NamaPerusahaan = m.app.namapt;
		m.PTKOTATTD = getSetting(0, 'company', 'Kota');
		
		// Set Tanggal dan Waktu
		m.formatDate = getSetting(0, 'company', 'FormatTanggalWS');
		m.formatTime = getSetting(0, 'company', 'FormatWaktuWS');
		m.formatDateApp = getSetting(0, 'company', 'FormatTanggalApp');
		m.formatTimeApp = getSetting(0, 'company', 'FormatWaktuApp');
		
		DefaultTanggal = CurrentDateTimeString(m.formatDateApp);
		m.DefaultDT = DefaultTanggal+" "+CurrentDateTimeString(m.formatTimeApp);
		m.DateFirst = "01/01/1900";
		
		// Set Hak Akses (usermenu dan tree expand)
		m.ArrUserMenu.removeAll();
		m.wsData = f_ArrDetail(arCoreData[4]);
		for(i=0;i<m.wsData.length;i++)
		{
			m.ArrUserMenu.addItem({
				mnmoduleid:m.wsData[i].f0,
				mnid:m.wsData[i].f1,
				mnname:m.wsData[i].f2,
				mnurl:m.wsData[i].f3,
				mnparent:m.wsData[i].f4,
				mntype:m.wsData[i].f5,
				mnlevel:m.wsData[i].f6,
				mnurutan:m.wsData[i].f7,
				mnactive:m.wsData[i].f8,
				mnviewopening:m.wsData[i].f9,
				mnpopup:m.wsData[i].f10,
				mnlebar:m.wsData[i].f11,
				mntinggi:m.wsData[i].f12,
				userid:m.wsData[i].f13,
				rmakses:m.wsData[i].f14,
				rmfavourite:m.wsData[i].f15
			});
			m.ArrTreeExpand.addItem({mnid:m.wsData[i].f0, mnexpand:0});  
		}
		// Hak Akses Custom
		m.wsData = f_ArrDetail(arCoreData[5]);
		m.ArrUserMenuCustom.removeAll();
		for(i=0;i<m.wsData.length;i++) 
		{
			m.ArrUserMenuCustom.addItem({	
				mid:m.wsData[i].f0,
				mname:m.wsData[i].f1,
				pcid:m.wsData[i].f2,
				pckode:m.wsData[i].f3,
				pcnama:m.wsData[i].f4,
				uruserid:m.wsData[i].f5,
				urakses:m.wsData[i].f6
			});
		}
		
		// Set Hak Akses Report
		m.wsData = f_ArrDetail(arCoreData[6]);
		m.ArrUserMenuReport.removeAll();
		for(i=0;i<m.wsData.length;i++){
			m.ArrUserMenuReport.addItem({
				userid:m.wsData[i].f0,
				rrrole:m.wsData[i].f1,
				rrmoduleid:m.wsData[i].f2,
				rrmenuid:m.wsData[i].f3,
				rritem:m.wsData[i].f4,
				rrakses:m.wsData[i].f5
			});
		}
		
		// Set Nomor
		m.ArrNomor.removeAll();
		m.wsData = f_ArrDetail(arCoreData[7]);
		for(i=0;i<m.wsData.length;i++){
			m.ArrNomor.addItem({ 	
				kodetabel:m.wsData[i].f0,
				moduleid:m.wsData[i].f1,
				menuid:m.wsData[i].f2,
				uraian:m.wsData[i].f5,
				catatan:m.wsData[i].f9
			});
		}
		
		// Set Periode Akuntansi
		m.ArrPeriodeAkuntansi.removeAll();
		m.wsData = f_ArrDetail(arCoreData[8]);
		for(i=0;i<m.wsData.length;i++)
		{
			m.ArrPeriodeAkuntansi.addItem({
				apkode:m.wsData[i].f0,
				aptahun:m.wsData[i].f1,
				apbulan:m.wsData[i].f2,
				apaktif:m.wsData[i].f3,
				aptutupperiode:m.wsData[i].f4
			});
		}
		
		// Set Report
		m.wsData = f_ArrDetail(arCoreData[9]);
		m.ArrReport.removeAll();
		for(i=0;i<m.wsData.length;i++){
			m.ArrReport.addItem({							
				rid:m.wsData[i].f0,
				rmoduleid:m.wsData[i].f1,
				rmenuid:m.wsData[i].f2,
				ritem:m.wsData[i].f3,
				rtitle:m.wsData[i].f4,
				rreportname:m.wsData[i].f5,
				rfilename:m.wsData[i].f6,
				rdefault:m.wsData[i].f7,
				rdata:m.wsData[i].f8,
				rcetak:m.wsData[i].f9,
				rsql:m.wsData[i].f10,
				rfrom:m.wsData[i].f11,
				rfilter:m.wsData[i].f12,
				rorderby:m.wsData[i].f13,
				rgroupby:m.wsData[i].f14,
				rquery:m.wsData[i].f15,
				raktif:m.wsData[i].f25,
				rurutan:m.wsData[i].f26
			});
		}
		
		// Kursor Manager, Remove Busy Kursor
		cursorManager.removeBusyCursor();
		
		// Setfocus di tree
		TreeKu.setFocus();
		
		// Set WebAccessKey
		WebAccessKey = arCoreData[10];
		
		// Set Sentence
		m.SentenceSearch = f_ArrDetail(arCoreData[11]);
		
		if(m.first == undefined){
			m.first = false;
			// buka dan buat home page "Welcome"
			openFormX("Home",0,"mod/m0/m0_welcome.swf",0,0,"t0","");
			
			// Tab page Home tidak bisa di close selamanya; 
			callLater(initNonClosableTab);
			
			// fungsi page home tidak bisa di close selamanya;
			function initNonClosableTab():void {
				MySuperTab.setClosePolicyForTab(0, SuperTab.CLOSE_NEVER);
			}
		}
		
		if(ubahasa != 'INA'){
			if(m.objTemp.reset)
				F_wsJson(sptParam+'M0_GetFileLibrary', 'File', m.url+'/app/libs/language/'+m.objTemp.comLanguage);
			else
				F_wsJson(sptParam+'M0_GetFileLibrary', 'File', m.url+'/app/libs/language/'+ubahasa);
		}else // jika tidak maka variabel bahasa di set kosong
			m.bahasa = new ArrayCollection;
		
		// jika Button Expand tampil, maka visible false dan panjang menu di kembalikan 250;
		if(BtnExpand.visible)
		{
			BoxLeft.width = 250;
			BtnExpand.visible = false;
		}
		
		// Set Setting POS
			m.ArrSettingPOS.removeAll();
			m.wsData = f_ArrDetail(arCoreData[13]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrSettingPOS.addItem({pcskategori:m.wsData[i].f0, pcsmodule:m.wsData[i].f1, 
					pcsgrup:m.wsData[i].f2, pcskode:m.wsData[i].f3, pcsnilai:m.wsData[i].f4,
					snama:m.wsData[i].f5, suraian:m.wsData[i].f6, 
					surutan:m.wsData[i].f7, stipedata:m.wsData[i].f8, sjenisinputan:m.wsData[i].f9, 
					scombodata:m.wsData[i].f10, pcnama:m.wsData[i].f11, modulename:m.wsData[i].f12});
			}
			
			// Set Default Nominal dan Number
			m.formatMinusAppPOS = getSettingPOS(0, 'app', 'FormatMinusApp');
			m.formatNominalPOS = getSettingPOS(0, 'app', 'FormatNominal').split('|');
			
			// Set Formatter Number, ex: 1000
			m.nfNumberPOS = new NumberFormatter;
			m.nfNumberPOS.groupingSeparator = m.formatNominalPOS[0];
			m.nfNumberPOS.decimalSeparator = "";
			m.nfNumberPOS.fractionalDigits = "0";
			m.nfNumberPOS.negativeNumberFormat = m.formatMinusApp;
			
			// Set Formatter Nominal, ex: 1.000,00
			m.nfNominalPOS = new NumberFormatter;
			m.nfNominalPOS.groupingSeparator = m.formatNominalPOS[0];
			m.nfNominalPOS.decimalSeparator = m.formatNominalPOS[1];
			m.nfNominalPOS.fractionalDigits = m.formatNominalPOS[2];
			m.nfNominalPOS.negativeNumberFormat = m.formatMinusAppPOS;
			
			
			// Set Tanggal dan Waktu
			m.formatDatePOS = getSettingPOS(0, 'app', 'FormatTanggalWS');
			m.formatTimePOS = getSettingPOS(0, 'company', 'FormatWaktuWS');
			m.formatDateAppPOS = getSettingPOS(0, 'app', 'FormatTanggalApp');
			m.formatTimeAppPOS = getSettingPOS(0, 'app', 'FormatWaktuApp');
			
		// Set Pos_Bonus_ItemSearch
			m.ArrPos_Bonus_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[14]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Bonus_ItemSearch.addItem({biid:m.wsData[i].f0, bikategori:m.wsData[i].f1, biidbarang:m.wsData[i].f2, bioperator:m.wsData[i].f3, bijml1:m.wsData[i].f4, bijml2:m.wsData[i].f5, bicustomtext1:m.wsData[i].f6, 
					bicustomtext2:m.wsData[i].f7, bicustomtext3:m.wsData[i].f8, bicustomtext4:m.wsData[i].f9, bicustomtext5:m.wsData[i].f10, bicustomint1:m.wsData[i].f11, bicustomint2:m.wsData[i].f12, bicustomint3:m.wsData[i].f13, 
					bicustomdbl1:m.wsData[i].f14, bicustomdbl2:m.wsData[i].f15, bicustomdbl3:m.wsData[i].f16, bicustomdate1:m.wsData[i].f17, bicustomdate2:m.wsData[i].f18, bicustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					bkode:m.wsData[i].f21, bnama:m.wsData[i].f22, btipe:m.wsData[i].f23, bsatuan:m.wsData[i].f24, bioperatornama:m.wsData[i].f25, bitgl1:m.wsData[i].f26, bitgl2:m.wsData[i].f27, binopromo:m.wsData[i].f28});
			}
			
		// Set Pos_Bonus_Item_DetailSetting
			m.ArrPos_Bonus_Item_DetailSetting.removeAll();
			m.wsData = f_ArrDetail(arCoreData[15]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Bonus_Item_DetailSetting.addItem({biid:m.wsData[i].f0, bid:m.wsData[i].f1, bkode:m.wsData[i].f2, bnama:m.wsData[i].f3, btipe:m.wsData[i].f4, bjenis:m.wsData[i].f5, bkategori:m.wsData[i].f6, bsatuan:m.wsData[i].f7, 
					bsatuandefault:m.wsData[i].f8, bhpp:m.wsData[i].f9, bbarcode:m.wsData[i].f10, bhargabeli:m.wsData[i].f11, bhppaverage:m.wsData[i].f12, bhargajual1:m.wsData[i].f13, bhargajual2:m.wsData[i].f14, 
					bhargajual3:m.wsData[i].f15, bhargajual4:m.wsData[i].f16, bhargajual5:m.wsData[i].f17, bdiskonjual1:m.wsData[i].f18, bdiskonjual2:m.wsData[i].f19, bdiskonjual3:m.wsData[i].f20, bdiskonjual4:m.wsData[i].f21, 
					bdiskonjual5:m.wsData[i].f22, bstok:m.wsData[i].f23, bstokbooking:m.wsData[i].f24, bmarginminimal:m.wsData[i].f25, brekpersediaan:m.wsData[i].f26, brekpenjualan:m.wsData[i].f27, brekreturpenjualan:m.wsData[i].f28, brekdiskonpenjualan:m.wsData[i].f29, 
					brekhargapokok:m.wsData[i].f30, brekreturpembelian:m.wsData[i].f31, brekdiskonpembelian:m.wsData[i].f32, brekkonsinyasi:m.wsData[i].f33, bserial:m.wsData[i].f34, bbatch:m.wsData[i].f35, bnilaisatuan:m.wsData[i].f36, 
					bnilaisatuandefault:m.wsData[i].f37, bsuplier:m.wsData[i].f38, bsuplierkode:m.wsData[i].f39, bsupliernama:m.wsData[i].f40, bnamafile:m.wsData[i].f41, bapanjang:m.wsData[i].f42, balebar:m.wsData[i].f43, batinggi:m.wsData[i].f44,
					bstokminimal:m.wsData[i].f45, bstokmaksimal:m.wsData[i].f46, breorder:m.wsData[i].f47, jml:m.wsData[i].f48});
			}
			
		// Set Pos_Substitution_ItemSearch
			m.ArrPos_Substitution_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[16]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Substitution_ItemSearch.addItem({siid:m.wsData[i].f0, sikategori:m.wsData[i].f1, siidbarang:m.wsData[i].f2, sioperator:m.wsData[i].f3, sijml1:m.wsData[i].f4, sijml2:m.wsData[i].f5, sicustomtext1:m.wsData[i].f6, 
					sicustomtext2:m.wsData[i].f7, sicustomtext3:m.wsData[i].f8, sicustomtext4:m.wsData[i].f9, sicustomtext5:m.wsData[i].f10, sicustomint1:m.wsData[i].f11, sicustomint2:m.wsData[i].f12, sicustomint3:m.wsData[i].f13, 
					sicustomdbl1:m.wsData[i].f14, sicustomdbl2:m.wsData[i].f15, sicustomdbl3:m.wsData[i].f16, sicustomdate1:m.wsData[i].f17, sicustomdate2:m.wsData[i].f18, sicustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					bkode:m.wsData[i].f21, bnama:m.wsData[i].f22, btipe:m.wsData[i].f23, bsatuan:m.wsData[i].f24, sioperatornama:m.wsData[i].f25, sitgl1:m.wsData[i].f26, sitgl2:m.wsData[i].f26, sinopromo:m.wsData[i].f27});
			}
			
		// Set Pos_Substitution_Item_DetailSetting
			m.ArrPos_Substitution_Item_DetailSetting.removeAll();
			m.wsData = f_ArrDetail(arCoreData[17]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Substitution_Item_DetailSetting.addItem({siid:m.wsData[i].f0, bid:m.wsData[i].f1, bkode:m.wsData[i].f2, bnama:m.wsData[i].f3, btipe:m.wsData[i].f4, bjenis:m.wsData[i].f5, bkategori:m.wsData[i].f6, bsatuan:m.wsData[i].f7, 
					bsatuandefault:m.wsData[i].f8, bhpp:m.wsData[i].f9, bbarcode:m.wsData[i].f10, bhargabeli:m.wsData[i].f11, bhppaverage:m.wsData[i].f12, bhargajual1:m.wsData[i].f13, bhargajual2:m.wsData[i].f14, 
					bhargajual3:m.wsData[i].f15, bhargajual4:m.wsData[i].f16, bhargajual5:m.wsData[i].f17, bdiskonjual1:m.wsData[i].f18, bdiskonjual2:m.wsData[i].f19, bdiskonjual3:m.wsData[i].f20, bdiskonjual4:m.wsData[i].f21, 
					bdiskonjual5:m.wsData[i].f22, bstok:m.wsData[i].f23, bstokbooking:m.wsData[i].f24, bmarginminimal:m.wsData[i].f25, brekpersediaan:m.wsData[i].f26, brekpenjualan:m.wsData[i].f27, brekreturpenjualan:m.wsData[i].f28, brekdiskonpenjualan:m.wsData[i].f29, 
					brekhargapokok:m.wsData[i].f30, brekreturpembelian:m.wsData[i].f31, brekdiskonpembelian:m.wsData[i].f32, brekkonsinyasi:m.wsData[i].f33, bserial:m.wsData[i].f34, bbatch:m.wsData[i].f35, bnilaisatuan:m.wsData[i].f36, 
					bnilaisatuandefault:m.wsData[i].f37, bsuplier:m.wsData[i].f38, bsuplierkode:m.wsData[i].f39, bsupliernama:m.wsData[i].f40, bnamafile:m.wsData[i].f41, bapanjang:m.wsData[i].f42, balebar:m.wsData[i].f43, batinggi:m.wsData[i].f44,
					bstokminimal:m.wsData[i].f45, bstokmaksimal:m.wsData[i].f46, breorder:m.wsData[i].f47, jml:m.wsData[i].f48});
			}
			
		// Set Pos_Additional_ItemSearch
			m.ArrPos_Additional_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[18]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Additional_ItemSearch.addItem({aiid:m.wsData[i].f0, aikategori:m.wsData[i].f1, aiidbarang:m.wsData[i].f2, aioperator:m.wsData[i].f3, aijml1:m.wsData[i].f4, aijml2:m.wsData[i].f5, aicustomtext1:m.wsData[i].f6, 
					aicustomtext2:m.wsData[i].f7, aicustomtext3:m.wsData[i].f8, aicustomtext4:m.wsData[i].f9, aicustomtext5:m.wsData[i].f10, aicustomint1:m.wsData[i].f11, aicustomint2:m.wsData[i].f12, aicustomint3:m.wsData[i].f13, 
					aicustomdbl1:m.wsData[i].f14, aicustomdbl2:m.wsData[i].f15, aicustomdbl3:m.wsData[i].f16, aicustomdate1:m.wsData[i].f17, aicustomdate2:m.wsData[i].f18, aicustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					bkode:m.wsData[i].f21, bnama:m.wsData[i].f22, btipe:m.wsData[i].f23, bsatuan:m.wsData[i].f24, aioperatornama:m.wsData[i].f25, aitgl1:m.wsData[i].f26, aitgl2:m.wsData[i].f27, ainopromo:m.wsData[i].f28});
			}
			
		// Set Pos_Additional_Item_DetailSetting
			m.ArrPos_Additional_Item_DetailSetting.removeAll();
			m.wsData = f_ArrDetail(arCoreData[19]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Additional_Item_DetailSetting.addItem({aiid:m.wsData[i].f0, bid:m.wsData[i].f1, bkode:m.wsData[i].f2, bnama:m.wsData[i].f3, btipe:m.wsData[i].f4, bjenis:m.wsData[i].f5, bkategori:m.wsData[i].f6, bsatuan:m.wsData[i].f7, 
					bsatuandefault:m.wsData[i].f8, bhpp:m.wsData[i].f9, bbarcode:m.wsData[i].f10, bhargabeli:m.wsData[i].f11, bhppaverage:m.wsData[i].f12, bhargajual1:m.wsData[i].f13, bhargajual2:m.wsData[i].f14, 
					bhargajual3:m.wsData[i].f15, bhargajual4:m.wsData[i].f16, bhargajual5:m.wsData[i].f17, bdiskonjual1:m.wsData[i].f18, bdiskonjual2:m.wsData[i].f19, bdiskonjual3:m.wsData[i].f20, bdiskonjual4:m.wsData[i].f21, 
					bdiskonjual5:m.wsData[i].f22, bstok:m.wsData[i].f23, bstokbooking:m.wsData[i].f24, bmarginminimal:m.wsData[i].f25, brekpersediaan:m.wsData[i].f26, brekpenjualan:m.wsData[i].f27, brekreturpenjualan:m.wsData[i].f28, brekdiskonpenjualan:m.wsData[i].f29, 
					brekhargapokok:m.wsData[i].f30, brekreturpembelian:m.wsData[i].f31, brekdiskonpembelian:m.wsData[i].f32, brekkonsinyasi:m.wsData[i].f33, bserial:m.wsData[i].f34, bbatch:m.wsData[i].f35, bnilaisatuan:m.wsData[i].f36, 
					bnilaisatuandefault:m.wsData[i].f37, bsuplier:m.wsData[i].f38, bsuplierkode:m.wsData[i].f39, bsupliernama:m.wsData[i].f40, bnamafile:m.wsData[i].f41, bapanjang:m.wsData[i].f42, balebar:m.wsData[i].f43, batinggi:m.wsData[i].f44,
					bstokminimal:m.wsData[i].f45, bstokmaksimal:m.wsData[i].f46, breorder:m.wsData[i].f47, jml:m.wsData[i].f48,
					customtext1:m.wsData[i].f49, customtext2:m.wsData[i].f50,customtext3:m.wsData[i].f51,customtext4:m.wsData[i].f52,customtext5:m.wsData[i].f53,
					customint1:m.wsData[i].f54, customint2:m.wsData[i].f55,customint3:m.wsData[i].f56,
					customdbl1:m.wsData[i].f57, customdbl2:m.wsData[i].f58,customdbl3:m.wsData[i].f59,
					customdate1:m.wsData[i].f60, customdate2:m.wsData[i].f61,customdate3:m.wsData[i].f62
				});
			}
			
		// Set Pos_Discount_ItemSearch
			m.ArrPos_Discount_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[20]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Discount_ItemSearch.addItem({dikategori:m.wsData[i].f0, diidbarang:m.wsData[i].f1, dioperator:m.wsData[i].f2, dijml1:m.wsData[i].f3, dijml2:m.wsData[i].f4, dikriteria:m.wsData[i].f5, dinilai:m.wsData[i].f6, 
					ditgl1:m.wsData[i].f7, ditgl2:m.wsData[i].f8, dijam1:m.wsData[i].f9, dijam2:m.wsData[i].f10, dicustomtext1:m.wsData[i].f11, dicustomtext2:m.wsData[i].f12, dicustomtext3:m.wsData[i].f13, 
					dicustomtext4:m.wsData[i].f14, dicustomtext5:m.wsData[i].f15, dicustomint1:m.wsData[i].f16, dicustomint2:m.wsData[i].f17, dicustomint3:m.wsData[i].f18, dicustomdbl1:m.wsData[i].f19, dicustomdbl2:m.wsData[i].f20, 
					dicustomdbl3:m.wsData[i].f21, dicustomdate1:m.wsData[i].f22, dicustomdate2:m.wsData[i].f23, dicustomdate3:m.wsData[i].f24, pcnama:m.wsData[i].f25, bkode:m.wsData[i].f26, bnama:m.wsData[i].f27, 
					btipe:m.wsData[i].f28, bsatuan:m.wsData[i].f29, dikriterianama:m.wsData[i].f30, dioperatornama:m.wsData[i].f31});
			}
			
		// Set Pos_Discount_Category_ItemSearch
			m.ArrPos_Discount_Category_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[21]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Discount_Category_ItemSearch.addItem({dcikategori:m.wsData[i].f0, dcikategoribarang:m.wsData[i].f1, dcioperator:m.wsData[i].f2, dcijml1:m.wsData[i].f3, dcijml2:m.wsData[i].f4, dcikriteria:m.wsData[i].f5, dcinilai:m.wsData[i].f6, 
					dcitgl1:m.wsData[i].f7, dcitgl2:m.wsData[i].f8, dcijam1:m.wsData[i].f9, dcijam2:m.wsData[i].f10, dcicustomtext1:m.wsData[i].f11, dcicustomtext2:m.wsData[i].f12, dcicustomtext3:m.wsData[i].f13, 
					dcicustomtext4:m.wsData[i].f14, dcicustomtext5:m.wsData[i].f15, dcicustomint1:m.wsData[i].f16, dcicustomint2:m.wsData[i].f17, dcicustomint3:m.wsData[i].f18, dcicustomdbl1:m.wsData[i].f19, dcicustomdbl2:m.wsData[i].f20, 
					dcicustomdbl3:m.wsData[i].f21, dcicustomdate1:m.wsData[i].f22, dcicustomdate2:m.wsData[i].f23, dcicustomdate3:m.wsData[i].f24, pcnama:m.wsData[i].f25, icnama:m.wsData[i].f26, dcikriterianama:m.wsData[i].f27, dcioperatornama:m.wsData[i].f28});
			}
			
		// Set Pos_Point_ItemSearch
			m.ArrPos_Point_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[22]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Point_ItemSearch.addItem({pikategori:m.wsData[i].f0, piidbarang:m.wsData[i].f1, pioperator:m.wsData[i].f2, pijml1:m.wsData[i].f3, pijml2:m.wsData[i].f4, pijmlpoint:m.wsData[i].f5, picustomtext1:m.wsData[i].f6, 
					picustomtext2:m.wsData[i].f7, picustomtext3:m.wsData[i].f8, picustomtext4:m.wsData[i].f9, picustomtext5:m.wsData[i].f10, picustomint1:m.wsData[i].f11, picustomint2:m.wsData[i].f12, picustomint3:m.wsData[i].f13, 
					picustomdbl1:m.wsData[i].f14, picustomdbl2:m.wsData[i].f15, picustomdbl3:m.wsData[i].f16, picustomdate1:m.wsData[i].f17, picustomdate2:m.wsData[i].f18, picustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					bkode:m.wsData[i].f21, bnama:m.wsData[i].f22, btipe:m.wsData[i].f23, bsatuan:m.wsData[i].f24, pioperatornama:m.wsData[i].f25, pitgl1:m.wsData[i].f26, pitgl2:m.wsData[i].f27, pinopromo:m.wsData[i].f28});
			}
			
		// Set Pos_Point_Category_ItemSearch
			m.ArrPos_Point_Category_ItemSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[23]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Point_Category_ItemSearch.addItem({pcikategori:m.wsData[i].f0, pcikategoribarang:m.wsData[i].f1, pcioperator:m.wsData[i].f2, pcijml1:m.wsData[i].f3, pcijml2:m.wsData[i].f4, pcijmlpoint:m.wsData[i].f5, pcicustomtext1:m.wsData[i].f6, 
					pcicustomtext2:m.wsData[i].f7, pcicustomtext3:m.wsData[i].f8, pcicustomtext4:m.wsData[i].f9, pcicustomtext5:m.wsData[i].f10, pcicustomint1:m.wsData[i].f11, pcicustomint2:m.wsData[i].f12, pcicustomint3:m.wsData[i].f13, 
					pcicustomdbl1:m.wsData[i].f14, pcicustomdbl2:m.wsData[i].f15, pcicustomdbl3:m.wsData[i].f16, pcicustomdate1:m.wsData[i].f17, pcicustomdate2:m.wsData[i].f18, pcicustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					icnama:m.wsData[i].f21, pcioperatornama:m.wsData[i].f22});
			}
			
		// Set Pos_Point_TransactionSearch
			m.ArrPos_Point_TransactionSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[24]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Point_TransactionSearch.addItem({ptkategori:m.wsData[i].f0, ptoperator:m.wsData[i].f1, ptjml1:m.wsData[i].f2, ptjml2:m.wsData[i].f3, ptjmlpoint:m.wsData[i].f4, ptcustomtext1:m.wsData[i].f5, ptcustomtext2:m.wsData[i].f6, 
					ptcustomtext3:m.wsData[i].f7, ptcustomtext4:m.wsData[i].f8, ptcustomtext5:m.wsData[i].f9, ptcustomint1:m.wsData[i].f10, ptcustomint2:m.wsData[i].f11, ptcustomint3:m.wsData[i].f12, ptcustomdbl1:m.wsData[i].f13, 
					ptcustomdbl2:m.wsData[i].f14, ptcustomdbl3:m.wsData[i].f15, ptcustomdate1:m.wsData[i].f16, ptcustomdate2:m.wsData[i].f17, ptcustomdate3:m.wsData[i].f18, pcnama:m.wsData[i].f19, ptoperatornama:m.wsData[i].f20, 
					pttgl1:m.wsData[i].f21, pttgl2:m.wsData[i].f22, ptnopromo:m.wsData[i].f23});
			}
			
			// Set Pos_Bonus_TransSearch
			m.ArrPos_Bonus_TransSearch.removeAll();
			m.wsData = f_ArrDetail(arCoreData[26]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Bonus_TransSearch.addItem({biid:m.wsData[i].f0, bikategori:m.wsData[i].f1, biidbarang:m.wsData[i].f2, bioperator:m.wsData[i].f3, bijml1:m.wsData[i].f4, bijml2:m.wsData[i].f5, bicustomtext1:m.wsData[i].f6, 
					bicustomtext2:m.wsData[i].f7, bicustomtext3:m.wsData[i].f8, bicustomtext4:m.wsData[i].f9, bicustomtext5:m.wsData[i].f10, bicustomint1:m.wsData[i].f11, bicustomint2:m.wsData[i].f12, bicustomint3:m.wsData[i].f13, 
					bicustomdbl1:m.wsData[i].f14, bicustomdbl2:m.wsData[i].f15, bicustomdbl3:m.wsData[i].f16, bicustomdate1:m.wsData[i].f17, bicustomdate2:m.wsData[i].f18, bicustomdate3:m.wsData[i].f19, pcnama:m.wsData[i].f20, 
					bkode:m.wsData[i].f21, bnama:m.wsData[i].f22, biipe:m.wsData[i].f23, bsatuan:m.wsData[i].f24, bioperatornama:m.wsData[i].f25, bitgl1:m.wsData[i].f26, bitgl2:m.wsData[i].f27, binopromo:m.wsData[i].f28});
			}
			
			// Set Pos_Bonus_Trans_DetailSetting
			m.ArrPos_Bonus_Trans_DetailSetting.removeAll();
			m.wsData = f_ArrDetail(arCoreData[27]);
			for(i=0;i<m.wsData.length;i++){
				m.ArrPos_Bonus_Trans_DetailSetting.addItem({biid:m.wsData[i].f0, bid:m.wsData[i].f1, bkode:m.wsData[i].f2, bnama:m.wsData[i].f3, btipe:m.wsData[i].f4, bjenis:m.wsData[i].f5, bkategori:m.wsData[i].f6, bsatuan:m.wsData[i].f7, 
					bsatuandefault:m.wsData[i].f8, bhpp:m.wsData[i].f9, bbarcode:m.wsData[i].f10, bhargabeli:m.wsData[i].f11, bhppaverage:m.wsData[i].f12, bhargajual1:m.wsData[i].f13, bhargajual2:m.wsData[i].f14, 
					bhargajual3:m.wsData[i].f15, bhargajual4:m.wsData[i].f16, bhargajual5:m.wsData[i].f17, bdiskonjual1:m.wsData[i].f18, bdiskonjual2:m.wsData[i].f19, bdiskonjual3:m.wsData[i].f20, bdiskonjual4:m.wsData[i].f21, 
					bdiskonjual5:m.wsData[i].f22, bstok:m.wsData[i].f23, bstokbooking:m.wsData[i].f24, bmarginminimal:m.wsData[i].f25, brekpersediaan:m.wsData[i].f26, brekpenjualan:m.wsData[i].f27, brekreturpenjualan:m.wsData[i].f28, brekdiskonpenjualan:m.wsData[i].f29, 
					brekhargapokok:m.wsData[i].f30, brekreturpembelian:m.wsData[i].f31, brekdiskonpembelian:m.wsData[i].f32, brekkonsinyasi:m.wsData[i].f33, bserial:m.wsData[i].f34, bbatch:m.wsData[i].f35, bnilaisatuan:m.wsData[i].f36, 
					bnilaisatuandefault:m.wsData[i].f37, bsuplier:m.wsData[i].f38, bsuplierkode:m.wsData[i].f39, bsupliernama:m.wsData[i].f40, bnamafile:m.wsData[i].f41, bapanjang:m.wsData[i].f42, balebar:m.wsData[i].f43, batinggi:m.wsData[i].f44,
					bstokminimal:m.wsData[i].f45, bstokmaksimal:m.wsData[i].f46, breorder:m.wsData[i].f47, jml:m.wsData[i].f48});
			}
		//=====================================================================================
		
		// Unload Page Login
		m.MlLogin.visible = false;
		
		m.wsData = arCoreData[25];
		if(m.wsData.length > 0)
		{
			m.POS_ = m.wsData;
			m.wsData = m.wsData.split(sptField);
			m.POS_computerIP = m.wsData[1];
			m.POS_port = m.wsData[3];
			m.POS_print = m.wsData[2];
			m.POS_struk = m.wsData[4];
			m.POS_jarak = m.wsData[7];
		}
		else
		{
			m.POS_ = "";
			m.POS_computerIP = "127.0.0.1";
			m.POS_port = 8484;
			m.POS_print = 1;
			m.POS_struk = 1;
			m.POS_jarak = 0;
		}
		m.POS_alamat1 = getSetting(0, "company", "Alamat1");
		m.POS_alamat2 = getSetting(0, "company", "Alamat2");
		m.queueHardware = new ArrayCollection;
		winHardware = new myHardware;
		tmrHardware.addEventListener(TimerEvent.TIMER, function(event:TimerEvent):void{
			winHardware.connect();
		});	
	}catch(e:Error){f_error('f_kondisiAwal', e)}
}

// fungsi get Current Date Time
public function CurrentDateTimeString(xFormatTanggal:String):String {         
	try
	{
		var CurrentDateTime:Date = new Date();
		var CurrentDF:DateFormatter = new DateFormatter();
		CurrentDF.formatString = xFormatTanggal;
		var DateTimeString:String = CurrentDF.format(CurrentDateTime);
		return DateTimeString;
	}catch(e:Error){f_error('CurrentDateTimeString', e)}
	return "";
}

// utk memformat tanggal
public function FormatDateTimeString(xTanggal:Date,xFormatTanggal:String):String {     
	try
	{
		var CurrentDateTime:Date = xTanggal;
		var CurrentDF:DateFormatter = new DateFormatter();
		CurrentDF.formatString = xFormatTanggal;
		var DateTimeString:String = CurrentDF.format(CurrentDateTime);
		return DateTimeString;
	}catch(e:Error){f_error('FormatDateTimeString', e)}
	return "";
}

//Bisa dipanggil anaknya
// ambil setting m0_setting
public function getSetting(ModuleID:int, Grup:String, Kode:String):String{
	try
	{
		for(i=0;i<m.ArrSetting.length;i++)
			if(parseInt(m.ArrSetting[i].smodule) == ModuleID && m.ArrSetting[i].sgrup == Grup && m.ArrSetting[i].skode == Kode)
				return m.ArrSetting[i].snilai; 
	}catch(e:Error){f_error('getSetting', e)}
	return '';
} 

// ambil setting m_12_pos_setting
public function getSettingPOS(ModuleID:int, Grup:String, Kode:String):String{
	try
	{
		for(i=0;i<m.ArrSettingPOS.length;i++){
			//Alert.show(m.ArrSettingPOS[i].pcsnilai)
			if(parseInt(m.ArrSettingPOS[i].pcsmodule) == ModuleID && m.ArrSettingPOS[i].pcsgrup == Grup && m.ArrSettingPOS[i].pcskode == Kode)
				return m.ArrSettingPOS[i].pcsnilai; 
		}
			
	}catch(e:Error){f_error('getSettingPOS', e)}
	return '';
}

// ambil report default, rdefault = 1
public function getReportDefault(xModuleID:int, xMenuID:int):String{
	try
	{
		for(i=0;i<m.ArrReport.length;i++)
			if(parseInt(m.ArrReport[i].rmoduleid) == xModuleID && m.ArrReport[i].rmenuid == xMenuID && m.ArrReport[i].rdefault == 1)
				return m.ArrReport[i].ritem+"|"+m.ArrReport[i].rtitle;
	}catch(e:Error){f_error('getReportDefault', e)}
	return '';
}

// jenis laporan form menu laporan
public function f_getReportMenuLaporan(xModuleID:int, xMenuID:int):ArrayCollection{
	try
	{
		m.arr = new ArrayCollection;
		for(i=0;i<m.ArrReport.length;i++){
			ob = m.ArrReport[i];
			if(ob.rmoduleid == xModuleID && ob.rmenuid == xMenuID && (ob.rdata == 0 || ob.rdata == 3 || ob.rdata == 4 || ob.rdata == 6) && int(ob.raktif))
				m.arr.addItem({ritem:ob.ritem,rtitle:ob.rtitle,rcetak:ob.rcetak,l:ob.rreportname,rid:ob.rid,rquery:ob.rquery});
		} 
		return m.arr;
	}catch(e:Error){f_error('f_getReportMenuLaporan', e)}
	return new ArrayCollection;
}

// jenis laporan form data
public function getReportItem(xModuleID:int, xMenuID:int):ArrayCollection{
	try
	{
		m.arr = new ArrayCollection;
		for(i=0;i<m.ArrReport.length;i++){
			ob = m.ArrReport[i];
			if(ob.rmoduleid == xModuleID && ob.rmenuid == xMenuID && (ob.rdata == 1 || ob.rdata == 3 || ob.rdata == 5 || ob.rdata == 6) && int(ob.raktif))
				for(j=0;j<m.ArrUserMenuReport.length;j++)
					if(m.ArrUserMenuReport[j].rrmoduleid == xModuleID && m.ArrUserMenuReport[j].rrmenuid == xMenuID && m.ArrUserMenuReport[j].rritem == ob.ritem){
						m.arr.addItem({ritem:ob.ritem, rtitle:ob.rtitle, rcetak:ob.rcetak, l:ob.rreportname, rid:ob.rid, rorderby:ob.rorderby, rgroupby:ob.rgroupby});
						break;
					}
		} 
		return m.arr;
	}catch(e:Error){f_error('getReportItem', e)}
	return new ArrayCollection;
}

// jenis laporan alert cetak ('dialog')
public function f_getReportAlertCetak(xModuleID:int, xMenuID:int):ArrayCollection{
	try
	{
		m.arr = new ArrayCollection;
		for(i=0;i<m.ArrReport.length;i++){
			ob = m.ArrReport[i];
			if(ob.rmoduleid == xModuleID && ob.rmenuid == xMenuID && (ob.rdata == 2 || ob.rdata == 4 || ob.rdata == 5 || ob.rdata == 6) && int(ob.raktif)){
				for(j=0;j<m.ArrUserMenuReport.length;j++)
					if(m.ArrUserMenuReport[j].rrmoduleid == xModuleID && m.ArrUserMenuReport[j].rrmenuid == xMenuID && m.ArrUserMenuReport[j].rritem == ob.ritem){
						m.arr.addItem({ritem:ob.ritem, rtitle:ob.rtitle, rcetak:ob.rcetak, l:ob.rreportname, rid:ob.rid, rorderby:ob.rorderby, rgroupby:ob.rgroupby});
						break;
					}
			}
		} 
		return m.arr;
	}catch(e:Error){f_error('f_getReportAlertCetak', e)}
	return new ArrayCollection;
}

// Fungsi Ambil Nomor berdasarkan kodeTabel/Sumber, utk mendapatkan Uraian/catatan/moduleid/menuid
public function getNomor(KodeTabel:String, Param:int = 0):String {
	try
	{
		for(i=0;i<m.ArrNomor.length;i++)
			if(m.ArrNomor[i].kodetabel == KodeTabel){
				if(Param == 0)
					return m.ArrNomor[i].uraian;
				else if(Param == 1)
					return m.ArrNomor[i].catatan;
				else if(Param == 2)
					return m.ArrNomor[i].moduleid;
				else if(Param == 3)
					return m.ArrNomor[i].menuid;
			}
	}catch(e:Error){f_error('getNomor', e)}
	return '';
}

// Fungsi utk memanggil komponen pencarian
public function F_cariDataCombo(paket:String, Filter:String = '', Sort:String = '', subject:String = '', checkbox:Boolean = false, kondisi:String = '', id:String = ''):void {
	try
	{
		if(m.bukacompencarian)
		{
			m.bukacompencarian2 = true;
			if(m.winC2 == undefined)
			{
				m.winC2 = new ComPencarian();
				PopUpManager.addPopUp(m.winC2, this, true);
				PopUpManager.removePopUp(m.winC2);
			}
			m.winC2.vSort = Sort;
			m.winC2.vPaket = paket;
			m.winC2.idmodule = idmod; 
			m.winC2.vTarget = subject;
			m.winC2.vFilterDefault = Filter;
			m.winC2.vModeCB = checkbox;
			m.winC2.kondisi = kondisi;
			m.winC2.vID = id;
			m.winC2.f_kondisiAwal();
		}
		else
		{
			winC.vSort = Sort;
			winC.vPaket = paket;
			winC.idmodule = idmod; 
			winC.vTarget = subject;
			winC.vFilterDefault = Filter;
			winC.vModeCB = checkbox;
			winC.kondisi = kondisi;
			winC.vID = id;
			winC.f_kondisiAwal();
		}
	}catch(e:Error){f_error('F_cariDataCombo', e)}
}


// Fungsi mencari menu dan langsung di buka, CTRL + M
public function f_cariMenu():void {
	try
	{
		winM = new ComSearchMenu(); 
		with (PopUpManager){
			addPopUp(winM,this, true);
			centerPopUp(winM);
		}
	}catch(e:Error){f_error('f_cariMenu', e)}
}

// Fungsi Import data utk form master data
public function F_importData(sumber:String):void {
	try
	{
		winImp = new ComImportData;
		winImp.sumber = sumber;
		with (PopUpManager){
			addPopUp(winImp, this, true);
			centerPopUp(winImp);
		}
	}catch(e:Error){f_error('F_importData', e)}
}

// Fungsi utk mengambil gambar/foto dari meuda seperti webcam
public function f_takePhoto():void {
	try
	{
		PopUpManager.addPopUp(winCam,this, true);
		PopUpManager.centerPopUp(winCam);
	}catch(e:Error){f_error('f_takePhoto', e)}
}

// Fungsi utk menampilkan alert atau informasi pesan cetak digunakan setelah input transaksi
public function F_alertCetak(mod:int, menu:int, sumber:String, idtransaksi:int, notransaksi:String, param1:String = '', kondisi:String = ''):void {
	try
	{
		PopUpManager.addPopUp(winAlertCetak, this, true);
		winAlertCetak.mod = mod;
		winAlertCetak.menu = menu;
		winAlertCetak.sumber = sumber;
		winAlertCetak.idtransaksi = idtransaksi;
		winAlertCetak.notransaksi = notransaksi;
		winAlertCetak.param1 = param1;
		winAlertCetak.kondisi = kondisi;
		winAlertCetak.setFocus();
		winAlertCetak.f_open();
		PopUpManager.centerPopUp(winAlertCetak);
	}catch(e:Error){f_error('f_takePhoto', e)}
}

// Fungsi menampilkan halaman bantuan
public function F_help(mod:int, menu:int, sub:String = ''):void{
	try
	{
		openFormX(l('Bantuan'), 0, 'mod/m0/m0_help.swf', 0, 0, 't4', '', [mod, menu, sub])
	}catch(e:Error){f_error('f_takePhoto', e)}
}

// Fungsi menampilkan tool Calculator, di gunakan di grid dg tekan F12 atau tekan icon calculator
public function F_toolCalculator(data:String = '', kolom:String = ''):void {
	winCal = new ComCalculator();
	winCal.data = data;
	winCal.kolom = kolom;
	with (PopUpManager){
		addPopUp(winCal, this, true);
		centerPopUp(winCal);
	}
}

// buka progress bar upload
public function F_openProgressUpload(jmlProgress:uint, atitle:String = "", close:Boolean = false):void
{
	m.winProgressUpload = new ComProgressUpload;
	m.winProgressUpload.jmlProgress = jmlProgress;
	m.winProgressUpload.atitle = atitle;
	m.winProgressUpload.close = close;
	PopUpManager.addPopUp(m.winProgressUpload, this, true);
	PopUpManager.centerPopUp(m.winProgressUpload);
}

// buka progress bar upload
public function F_openProgressHapusData(jmlProgress:uint):void
{
	m.winProgressUpload = new ComProgressUpload;
	m.winProgressUpload.jmlProgress = jmlProgress;
	m.winProgressUpload.hapusdata = "ya";
	PopUpManager.addPopUp(m.winProgressUpload, this, true);
	PopUpManager.centerPopUp(m.winProgressUpload);
}

// send progress bar upload
public function F_sendProgress(data:String, status:Boolean = true):void
{
	m.winProgressUpload.f_sendProgress(data, status);
}

// tampilkan komponen ganti bahasa
public function F_showComLanguage():void {
	winLang = new ComLanguage(); 
	with (PopUpManager){
		addPopUp(winLang, this, true);
		centerPopUp(winLang);
	}
}

// tampilkan form administrasi user detail utk ganti profil user kecuali (role, expired date, aktif)
public function f_ChangeUserProfil():void {
	openFormX("Administrasi User Detail", 1, "mod/m0/m0_AdministrasiUserDetail.swf", 500, 400, "t1", "parent");
}

// buka form baru dg filter module, menu
public function openForm(ModuleID:int, MenuID:int, Filter:String="", ArrFilter:Array = null):void 			
{
	for(i=0;i<m.ArrUserMenu.length;i++)
		if(parseInt(m.ArrUserMenu[i].mnmoduleid) == ModuleID && parseInt(m.ArrUserMenu[i].mnid) == MenuID){
			m.o = m.ArrUserMenu[i];
			openFormX(m.o.mnname, m.o.mnpopup, m.o.mnurl, m.o.mntinggi, m.o.mnlebar, "t"+m.o.mntype, Filter, ArrFilter);
			break;
		}
} 


// fungsi hak akses, berdasarkan moduleID, menuID
public function apaBisaAkses(ModuleID:int, MenuID:int, AksiID:int=0, errmessage:int = 1):Boolean
{
	var i:int, isFound:Boolean=false, BisaAkses:Boolean=false;
	
	for(i=0;i<m.ArrUserMenu.length;i++)
	{
		if(parseInt(m.ArrUserMenu[i].mnmoduleid) == ModuleID && parseInt(m.ArrUserMenu[i].mnid) == MenuID)
		{
			isFound=true; break;
		}
	}
	if(isFound == false)
	{
		if(Boolean(errmessage) == true)
			Alert.show(l("Anda tidak punya akses ke Module")+" " + ModuleID.toString() + " Menu " + MenuID.toString(), l("Informasi"));
	}
	else if(isFound == true) 
	{
		BisaAkses=true;
		///Cek by AksiID : Ke-1=Insert/Update, 2=Delete, 3=OpenForm/GetData, 4=Report, 5=Setting, 6=Approfal, 7=Draft 8=Close
		//
		if(AksiID > 0) 
		{
			var strAkses:String = m.ArrUserMenu[i].rmakses, strAksesCompare:String = strAkses.slice(AksiID - 1, AksiID);
			if(strAksesCompare == "0")
			{
				BisaAkses=false;	
				if(Boolean(errmessage) == true)
					Alert.show(l("Anda tidak punya akses ke Module")+" " + ModuleID.toString() + " Menu " + MenuID.toString(), l("Informasi"));
			}
		}
	}
	return BisaAkses;
}

// ambil setting Hak akses custom
public function getHakAksesCustom(ModuleID:int, IDCustom:int):Boolean
{
	for(i=0;i<m.ArrUserMenuCustom.length;i++)
		if(parseInt(m.ArrUserMenuCustom[i].mid) == ModuleID && parseInt(m.ArrUserMenuCustom[i].pcid) == IDCustom && m.ArrUserMenuCustom[i].urakses == 1 && m.ArrUserMenuCustom[i].uruserid == userid)
			return true; 
	
	return false;
} 

// log out program
public function f_logout():void
{
	m.objTemp.reset = false;
	m.a = idmod.split('popUpLoader');
	if(m.a.length == 2)
		for(i=0;i<m.a[1];i++)
			closeFormPopUp((i+1));
	PopUpManager.removePopUp(winC);
	if(m.NewSentence == undefined)
		F_wsSimpan(sptParam+'logout', 'M0_Logout', userid, false, m.kodeapp);
	else{
		m.a = [];
		for(i=0;i<m.NewSentence.length;i++){
			m.a[i] = [0, m.NewSentence[i].d, m.NewSentence[i].t];
		}
		F_wsSimpan(sptParam+'languageSystem', 'M0_SentenceSimpan', userid, false, F_splitWs(m.a))
	}
}

// get menu log out, tanggal, langguage
public function f_iconMenuHandler(menu:String):void
{
	switch(menu){
		case "Log Out":
			f_logout();
			break;
		case "Tanggal":
			showDefaultTanggal();	
			break;
		case "Language":
			F_showComLanguage();
			break;
		case "UserProfil":
			f_ChangeUserProfil();
			break;
	}
}

// menampilkan komponen default tanggal
public function showDefaultTanggal():void
{
	PopUpManager.centerPopUp(ComDefaultTanggal(PopUpManager.createPopUp(this, ComDefaultTanggal, true)));
}

//Induk : Treeview menu
protected function f_treeClick(event:ListEvent):void
{
	m.selectedNode=Tree(event.target).selectedItem as XML;
	f_treeExecution();
}

// tree click aksi, maka menampilkan form atau jika ada formnya maka di fokuskan
public function f_treeExecution():void
{
	var StrLabel:String = m.selectedNode.@l;
	var IsPopup:int = m.selectedNode.@p;
	var StrURL:String = m.selectedNode.@u;
	var Tinggi:int = m.selectedNode.@tg;
	var Lebar:int = m.selectedNode.@lb;
	var Tipe:String = m.selectedNode.@t;
	var xMenuID:int = m.selectedNode.@id;
	var modnam:String;
	currentMenuID = xMenuID;
	m.currentMenuIDT = m.selectedNode.@idt;
	if(m.selectedNode.@t!='99'){
		if(CekTab(StrLabel, Tipe)){
			openFormX(StrLabel, IsPopup, StrURL, Tinggi, Lebar, Tipe);
		}else{
			//kalo sudah dipanggil langsung sorot ke tab
			MySuperTab.selectedIndex = CariIndexTab(StrLabel, Tipe);
		}
	}else{
		var x:int=m.ArrTreeExpand.length;
		for(i=0;i<x;i++)
			if(m.ArrTreeExpand[i].mnid==xMenuID.toString()){
				if(m.ArrTreeExpand[i].mnexpand==0){
					TreeKu.expandItem(m.selectedNode,true,false);
					m.ArrTreeExpand.setItemAt({mnid:m.ArrTreeExpand[i].mnid,mnexpand:1},i);
				}
				else{
					TreeKu.expandItem(m.selectedNode,false,false);
					m.ArrTreeExpand.setItemAt({mnid:m.ArrTreeExpand[i].mnid,mnexpand:0},i);
				}
				break;
			}
	}
}

// mengembalikan tambah filter biasanya di pakai pada form data
public function TambahFilter(FilterLama:String = "" ,FilterBaru:String = ""):String
{
	if(FilterBaru.length == 0)
		return FilterLama;
	else if(FilterLama.length == 0 || FilterLama == null)
		return  "(" + FilterBaru + ")";
	else if(FilterLama.length > 0)
		return  FilterLama + " AND (" + FilterBaru + ")";
	return '';
}

//Untuk induk saja
//Button Collapse Click maka BoxLeft panjang 0 dan tampilkan Button Expand
protected function BtnCollapse_clickHandler(event:MouseEvent):void
{
	BoxLeft.width = 0;
	BtnExpand.visible=true;
}

//Button Expand klik maka Boxleft panjang 250 dan hidden BUtton expand
protected function BtnExpand_clickHandler(event:MouseEvent):void
{
	BoxLeft.width = 250;
	BtnExpand.visible = false;
}

//jika hdividebox di geser2
protected function hdividedbox1_dividerDragHandler(event:DividerEvent):void
{
	if(BoxLeft.width < 25)
		BtnExpand.visible = true;
	else
		BtnExpand.visible=false;
}

// close form PopUP berdasarkan level pop up itu sendiri
public function closeFormPopUp(levelPopUp:int=1):void
{
	var namaLoader:String="popUpLoader"+levelPopUp;
	var namaWindow:String="winPopUp"+levelPopUp;
	m[namaLoader].url= "";
	m[namaLoader].unloadModule();
	m.modTemp[levelPopUp] = '';
	idmod = m.modTemp[levelPopUp-1];
	if(levelPopUp > 1)
		idmodcurr = m.modTemp[levelPopUp-2];
	else
		idmodcurr = '';
	if(m.hasOwnProperty(idmod))
		if(m[idmod].hasOwnProperty('child'))
			o = m[idmod].child;
	PopUpManager.removePopUp(m[namaWindow]); 
}

// fungsi replace
public static function replaceAll(strSource:String, strReplaceFrom:String, strRepalceTo:String):String 
{
	return strSource.split(strReplaceFrom).join(strRepalceTo);
}

// Applikasi saat ada inputan dari keyboard maka ...
protected function App_keyDown(event:KeyboardEvent):void
{
	// berfungsi saat setelah berhasil login
	if(m.login == true)
		switch(event.keyCode){
			// jika di tekan F8 maka fokus ke TreeKU atau menu Tree
			case Keyboard.F8:
				TreeKu.selectedIndex = 0;
				TreeKu.setFocus();
				break;
			// jika di tekan M dan ctrlKey maka keluar window pencarian menu
			case Keyboard.M:
				if(event.ctrlKey)
					f_cariMenu();
				break;
			// jika di tekan L dan ctrlKey maka akan keluar konfirmasi atau alert log out
			case Keyboard.L:
				if(event.ctrlKey)
					Alert.show(l('Apakah Anda akan logout ?'), l('Informasi'), 3, null, function(e:Object):void{	
						if(e.detail == Alert.YES)
							f_iconMenuHandler('Log Out');
					});
				break;
			// jika di tekan W dan ctrlKey maka akan keluar konfirmasi atau alert current form
			case Keyboard.W:
				if(event.ctrlKey)
					Alert.show(l('Tutup form ini ?'), l('Informasi'), 3, null, function(e:Object):void{	
						if(e.detail == Alert.YES)
							f_tabCloseActive();
					});
				break;
			// jika di tekan D dan ctrlKey maka akan keluar window pengganti tanggal default program
			case Keyboard.D:
				if(event.ctrlKey)
					f_iconMenuHandler('Default Tanggal');
				break;
		}
}

// tree menu module saat ada inputan dari kyeboard
protected function f_treeKeyDown(event:KeyboardEvent):void
{
	// jika di tekan Enter maka akan expan module tersebut
	if(event.keyCode == Keyboard.ENTER){
		m.selectedNode=Tree(event.target).selectedItem as XML;
		f_treeExecution();
	}
}

// hasil dari webservice dan download file json
public function f_result():void
{
	if(m.mulai)
	{
		//validasi row grid, bila di definisikan
		if(m.rowCari != undefined){
			rowIndex = m.rowCari;
			m.rowCari = undefined;
		}
		
		//Tmr Busy di hentikan
		m.tmrBusy.stop();
		
		// Hidden Goup Busy atau hilangkan gambar yang menutupi aplikasi
		grpBusy.visible = false;
		
		// set Goup Busy Alpha jadi 0.25
		grpGEBusy.alpha = 0.25;
		
		// Jika Object Focus bernilai tidak Null
		if(m.objFocus != null)
			if(String(m.objFocus).split('.')[0] == 'MyERPPlus' || String(m.objFocus).split('winPopUp').length > 1)
				m.objFocus.setFocus();
	}
	if(subject != null){
		// jika invalid webaccesskey maka logout otomatis
		if(wsErrmessage == "Invalid Website Access Key." && wsTarget != "M0_Logout"){
			f_getReloginForm();
			//f_logout();
		}else if(subject.split(sptParam).length == 1){
			f_refreshIDmodule();
			mod('f_wsHasil');
			if(String(m.objFocus).split('winPopUp').length > 1)
				F_validasiTrueWin()
			else
				F_validasiTrue();
		}else{
			f_wsHasil();
		}
	}else Alert.show(l("Subject Null"), l("Informasi"));
}

// untuk split hasil dari webservice
public function f_setVariableWS():void{
	// split ob_resultWs dg spt Param
	vArrResult = ob_resultWs.split(sptParam);
	
	// Split ArrResult dg sptSubParam untuk mendapakan Result, Paging dan data
	vResult = String(vArrResult[0]).split(sptSubParam);
	vPaging = String(vArrResult[1]).split(sptSubParam);
	vData = String(vArrResult[2]).split(sptSubParam);
	
	// Set Target, SUcces, Errmessage, Errstep dan idtransaksi
	wsTarget = vResult[0];
	wsSuccess = Boolean(int(vResult[1]));
	wsErrmessage = vResult[2]; 
	wsErrstep = vResult[3];
	wsIdtransaksi = vResult[4];
	
	// set Ispaging, IsNext, IsPrev, CurPage, CountRow
	wsIspaging = Boolean(int(vPaging[0]));
	wsIsNext = Boolean(int(vPaging[1]));
	wsIsPrev = Boolean(int(vPaging[2]));
	wsCurPage = int(vPaging[3]);
	wsCountRow = int(vPaging[4]);
	
	// Set Panjang Kolom
	vKolomLength = vData[0].split(sptRow)[0].split(sptField).length;
	
	// jika panjang result sama dengan 3 maka ...
	if(vArrResult.length == 3){
		// set Data Utama ke wsArrUtama
		wsArrUtama = f_ArrDetail(vData[0]);
		// jika panjang data lebih dari 1 maka set Data Detail ke variable wsArrDetail
		if(vData.length >1)
			wsArrDetail = f_ArrDetail(vData[1]);
	}else{
		// Set Nama Kolom
		vNamaKolom = String(vArrResult[3]).split(sptSubParam);
		// set Data Utama ke wsArrUtama
		wsArrUtama = f_SplitData(vNamaKolom[0], vData[0]);
		// jika panjang data lebih dari 1 maka set Data Detail ke variable wsArrDetail
		if(vData.length >1)
			wsArrDetail = f_SplitData(vNamaKolom[1], vData[1]);
	}
	
	// set Validasi True 
	wsValidasi = true;
}

// fungsi hasil dari webservice
public function f_wsHasil():void
{
	try{//buar variable j itu int, dan a itu arracollection
		var j:int, a:ArrayCollection = new ArrayCollection;
		// set variable subject
		subject = subject.split(sptParam).join("");
		//switch berdasarkan subject
		switch(subject){
			case "M0_GetLibraryGrid":
				m.grid = F_jsonDecode(ob_resultWs);
				
				// Ganti status CheckingFile
				m.o.text = "Downloading 2 of 3 : Report Configurations";
				
				F_wsSearch(sptParam+"M0_GetLibraryReport", "M0_GetLibraryReport");
				break;
			case "M0_GetLibraryReport":
				m.laporan = F_jsonDecode(ob_resultWs).form;
				
				// Ganti status CheckingFile
				m.o.text = "Downloading 3 of 3 : Statistic Configurations";
				
				F_wsSearch(sptParam+"M0_GetLibraryStatistic", "M0_GetLibraryStatistic");
				break;
			case "M0_GetLibraryStatistic":
				m.statistic = F_jsonDecode(ob_resultWs).form;
				
				// Cek bahasa user tidak sama dengan bhs indonesia(default) , maka load file bahasanya (json)
//				if(ubahasa != 'INA'){
//					if(m.objTemp.reset)
//						F_wsJson(sptParam+'M0_GetFileLibrary', 'File', m.url+'/app/libs/language/'+m.objTemp.comLanguage);
//					else
//						F_wsJson(sptParam+'M0_GetFileLibrary', 'File', m.url+'/app/libs/language/'+ubahasa);
//				}else // jika tidak maka variabel bahasa di set kosong
//					m.bahasa = new ArrayCollection;
				
				
				// Ganti status CheckingFile
				m.o.text = "Downloading Done";
				
				// Label Please Visible false
				m.l.visible = false
				m.mulai = true;
				
				// Inisialisasi parent
				f_persiapanInduk();
				break; 
			// jika import data maka set winImp  kolom dan dg dataprovider
			case 'importdata':
				winImp.kolom = vNamaKolom[0].split(sptField);
				winImp.dg.dataProvider = wsArrUtama;
				break;
			// jika checckdb
			case 'checkdb':
				/* m['MlLogin'].lblDBVer.text = wsArrUtama[0].f0; */
				break;
			// jika login
			case 'login':
				// jika sukses maka panggil fungsi kondisi awal
				if(wsSuccess == true){
					m.login = true;
					if(m.pertama)
					{
						m.pertama = false;						
						PopUpManager.addPopUp(winC, this, true);
						PopUpManager.removePopUp(winC);
					}
					f_kondisiAwal();
				}else{
					// jika gagal dan errstep sama dengan 1 maka keluar pesan konfirmasi
					if(int(wsErrstep) == 1){
						// Do you want to continue ? jika di klik yes maka akan login dan sesi yang sebelumnya akan ke log out otomatis
						// jika di klik NO maka tidak jadi login dan sesi sebelumnya masih tetep jalan
						Alert.show(wsErrmessage+"\nDo you want to continue ?", 'Confirmation', 3, null, function(e:Object):void{	
							if(e.detail == Alert.YES)
								// jika ya maka panggil ws login lagi, untuk mengakhiri sesi sebelumnya dan mulai sesi yang baru
								F_wsSimpan(sptParam+'login', 'M0_Login', 0, false, F_splitArrayToString(["3k1StR5601aDz2N74Go3", "66f755e7de426692105cb494e79d7bdc", m.MlLogin.txtUsername.text, m.MlLogin.txtPassword.text, m.kodeapp, 1]), '');
							else if(e.detail == Alert.NO){
								// jika tidak maka aktifkan lagi text input username, text input password dan button login
								m['MlLogin'].txtUsername.enabled = true;
								m['MlLogin'].txtPassword.enabled = true;
								m['MlLogin'].BtnLogin.enabled = true;
							}
						});	
					}else{
						// jika errstep tidak sama dengan 1 maka ...
						// aktifkan lagi text input username, text input password dan button login
						m['MlLogin'].txtUsername.enabled = true;
						m['MlLogin'].txtPassword.enabled = true;
						m['MlLogin'].BtnLogin.enabled = true;
						
						if(wsErrmessage.toLocaleLowerCase().split("username").length == 2){
							m['MlLogin'].txtPassword.text = "";
							m['MlLogin'].txtUsername.setFocus();
						}else if(wsErrmessage.toLocaleLowerCase().split("password").length == 2){
							m['MlLogin'].txtPassword.text = "";
							m['MlLogin'].txtPassword.setFocus();
						}
						// tampilkan pesan error
						Alert.show(wsErrmessage, l("Informasi"));
					}
				}
				break;
			//jika login2 (cadangan) , atau kondisi login pertama kali ada.
			case 'login2':
				if(wsSuccess == true){
					m.login = true;
					m.jmllogout += 1;
					f_kondisiAwal();
					PopUpManager.removePopUp(winRelogin);
				}else{
					if(int(wsErrstep) == 1){
						Alert.show(wsErrmessage+"\nDo you want to continue ?", 'Confirmation', 3, null, function(e:Object):void{	
							if(e.detail == Alert.YES)
								F_wsSimpan(sptParam+'login2', 'M0_Login', 0, false, F_splitArrayToString(["3k1StR5601aDz2N74Go3", "66f755e7de426692105cb494e79d7bdc", winRelogin.txtUsername.text, winRelogin.txtPassword.text, m.kodeapp, 1]), '');
							else if(e.detail == Alert.NO){
								winRelogin.txtPassword.enabled = true;
								winRelogin.BtnLogin.enabled = true;
							}
						});	
					}else{
						winRelogin.txtPassword.enabled = true;
						winRelogin.BtnLogin.enabled = true;
						Alert.show(wsErrmessage, l("Informasi"));
					}
				}
				break;
			case 'M0_GetFileLibrary':
				if(m.objTemp.reset == true)
					m.objTemp.comLanguage = null;
				m.bahasa = wsJson.property; 
				break;
			case 'bukacompencarian':
				if(m.bukacompencarian2)
				{
					PopUpManager.addPopUp(m.winC2, this, true);
					PopUpManager.centerPopUp(m.winC2);
				}
				else
				{
					m.bukacompencarian = true;
					PopUpManager.addPopUp(winC, this, true);
					PopUpManager.centerPopUp(winC);
				}
			case 'caridatacombo':
				if(m.bukacompencarian2)
				{
					m.winC2.f_persiapan(); 
				}
				else
				{
					winC.f_persiapan(); 
				}
				break;
			case 'caridatatransaksi':
				m[idmod].child.o.bt.f_setData();
				break;
			case 'updateStatus':
				if(wsSuccess == false){
					Alert.show(wsErrmessage, l("Informasi"));
					return;
				}
			case 'tampilForm':
				f_refreshIDmodule()
				if(wsSuccess == true){
					o = m[idmod].child;
					for(i=0;i<wsArrUtama.length;i++) 
						if(wsArrUtama[i][m.sumber+"modifikasiusernama"] == "")
							wsArrUtama[i][m.sumber+"modifikasitgl"] = ""
					o.dg.dataProvider = wsArrUtama; 
					o.dg.setSelectedCell(0,0);
					rowIndex = 0;
					o.imgfirst.enabled = wsIsPrev;
					o.imgprevious.enabled = wsIsPrev;
					o.imgnext.enabled = wsIsNext;
					o.imglast.enabled = wsIsNext;
					o.txtpaging.text = wsCurPage;
					for(i=0;i<o.dg.dataProviderLength;i++)
						o.dg.dataProvider[i].no = (((wsCurPage-1)*20)+(i+1));
					F_dgRefresh(o.dg);
					o.dg.setFocus();
					o.dg.setSelectedCell(0,0);
					if(o.hasOwnProperty('f_grid_change'))o.f_grid_change();
				}else m[idmod].child.dg.dataProvider.removeAll();
				break; 
			case 'tampilFDJurnalVocher':
				if(wsSuccess == true){
					m[idmod].child.o.jt = new m0_JurnalTerkait;
					m[idmod].child.o.jt.percentHeight = 100;
					m[idmod].child.o.jt.percentWidth = 100;
					m[idmod].child.addElement(m[idmod].child.o.jt);
					m[idmod].child.o.jt.jenis = "Jurnal";
					m[idmod].child.o.jt.persiapan(this);
					m[idmod].child.o.jt.ttwgridterkait.setFocus();
					m[idmod].child.o.jt.gridjv.setFocus();
					m[idmod].child.o.jt.gridjv.setSelectedCell(0,0);
				}else Alert.show(wsErrmessage, l("Informasi"));
				break; 
			case 'tampilFDTransaksiTerkait':
				if(wsSuccess == true){
					m[idmod].child.o.jt = new m0_JurnalTerkait;
					m[idmod].child.o.jt.percentHeight = 100;
					m[idmod].child.o.jt.percentWidth = 100;
					m[idmod].child.addElement(m[idmod].child.o.jt);
					m[idmod].child.o.jt.jenis = "Terkait";
					m[idmod].child.o.jt.persiapan(this);
					m[idmod].child.o.jt.ttwgridterkait.setFocus();
					m[idmod].child.o.jt.gridtts.setFocus();
					m[idmod].child.o.jt.gridtts.setSelectedCell(0,0);
				}else Alert.show(wsErrmessage, l("Informasi"));
				break; 
			case 'bukaTransaksi':
				if(wsSuccess == true){
					m[idmod].child.o.bt.f_persiapan();
					m[idmod].child.o.bt.title = 'Buka Transaksi '+m.ArrTab[MySuperTab.selectedIndex].label;
				}else Alert.show(wsErrmessage);
				break; 
			case 'comLanguage':
				m.objTemp.m0_SettingLanguage = wsJson;
				registerGrid([winLang.dg], 'data');
				F_wsSearch(sptParam+'comLanguageTampil', 'M0_LanguageSearch', 0, 0, '', 'lnama');
				break;
			case 'comLanguageTampil':
				winLang.dg.dataProvider.removeAll();
				if(wsSuccess == true)
					for(i=0;i<wsArrUtama.length;i++)
						with(wsArrUtama[i])
							if(lgambar != '')
								winLang.dg.dataProvider.addItem({lkode:lkode, lnama:lnama, laktif:laktif, lkode_source:m.url+'/files/f0/language/'+lgambar});
							else
								winLang.dg.dataProvider.addItem({lkode:lkode, lnama:lnama, laktif:laktif, lkode_source:''});
				F_dgRefresh(winLang.dg);
				break;
			case 'logout':
				m.mod = {};
				PopUpManager.removePopUp(winRelogin);
				f_tabCloseAll();
				TreeKu.dataProvider = null;
				BtnLinkJudul.label = "";
				m.jmllogout += 1;
				m.MlLogin.f_formLoad();
				m.MlLogin.visible = true;
				break;
			case 'languageSystem':
				m.NewSentence = undefined;
				F_wsSimpan(sptParam+'logout', 'M0_Logout', userid, false, m.kodeapp);
				break;
			case 'getFormData':
				registerGrid([m[idmod].child.dg], 'data');
				mod('f_persiapan');
				break;
			case 'terkaitFormMaster':
				if(wsSuccess){
					m[idmod].child.dg.dataProvider = wsArrUtama;
					m[idmod].child.dg.setSelectedCell(0,0);
					rowIndex = 0; 
					m[idmod].child.txtpaging.text = wsCurPage;
				}else Alert.show(wsErrmessage, l('Infromasi'));
				break;
			case "alertCetak":
				if(wsSuccess){
					winAlertCetak.f_print();
				}else Alert.show(wsErrmessage, l("Informasi"));
				break;
			case "TampilkanGambar":
				if(wsSuccess){
					var gambar:m0_Picture ;
					gambar = new m0_Picture();
					PopUpManager.addPopUp(gambar, this, true);
					PopUpManager.centerPopUp(gambar);
					gambar.setFocus();
				}else Alert.show(wsErrmessage, l("Informasi"));
				break;
		} 
	}catch(e:Error){f_error('f_wsHasil', e)}
}

protected function grpBusy_clickHandler(event:MouseEvent):void
{
	if(grpBusy.visible == true && grpGEBusy.alpha <= 0.5)
		grpGEBusy.alpha += 0.1;
}

protected function vx_resizeHandler(event:ResizeEvent):void
{
	vx.height = BoxGrupMenu.height-28;
	vx.validateNow();
	if(event.oldHeight > BoxGrupMenu.height){
		// turun
		if(m.hasOwnProperty('btn'))
			if(m.btn[int(vx.height/30)] != null){
				for(i=int(vx.height/30);i<m.btn.length;i++){
					if(m.btn.length > (i+1)){
						m.btn[i+1].height = 28;
						m.btn[i+1].width = 28;
						hxbottom.addChild(m.btn[i+1]);
					}
				}
				if(((vx.height/30)-int(vx.height/30)) <= 0.1){
					m.btn[int(vx.height/30)].height = 28;
					m.btn[int(vx.height/30)].width = 28;
					hxbottom.addChildAt(m.btn[int(vx.height/30)], 0);
				} 
				if(bHxbottom == false){
					bHxbottom = true
					hxbottom.removeChild(btnbottom);
				}
			}
	}else{
		// naik
		if(m.hasOwnProperty('btn'))
			if(m.btn[int(vx.height/30)] != null){
				for(i=0;i<int(vx.height/30);i++){
					if(m.btn.length > (i+1)){
						m.btn[i].height = 30;
						m.btn[i].percentWidth = 100;
						vx.addChildAt(m.btn[i], i);
					}
				}
				if(((vx.height/30)-int(vx.height/30)) > 0.1){
					m.btn[int(vx.height/30)].height = 30;
					m.btn[int(vx.height/30)].percentWidth = 100;
					vx.addChild(m.btn[int(vx.height/30)]);
				}
				if(hxbottom.numChildren == 0){
					bHxbottom = false;
					hxbottom.addChild(btnbottom);
				}
			}
	}
}

protected function createAndShow():void {
	var myMenu:Menu = Menu.createMenu(null, mbProfil, false);
	myMenu.labelField="@l";
	myMenu.iconField="@i";
	myMenu.addEventListener(MenuEvent.ITEM_CLICK,  itemClickInfo);
	myMenu.show(this.width-145, 24);
	
	function itemClickInfo(e:MenuEvent):void
	{
		f_iconMenuHandler(e.label);
	}
}

protected function imgscreen_clickHandler(event:MouseEvent):void
{
	if(m.fullscreen == false){
		m.fullscreen = true;
		stage.displayState = StageDisplayState.FULL_SCREEN
	}else{
		m.fullscreen = false;
		stage.displayState = StageDisplayState.NORMAL;
	}
}

// Event - event Module
protected function MySuperTab_changeHandler(event:IndexChangedEvent):void 
{ 
	m.oldTabIndex = event.oldIndex; 
	m.newTabIndex = event.newIndex;
	idmod = m.ArrTab[CariIndexTab(MySuperTab.getChildAt(event.newIndex)['label'], f_toolTipeMySuperTab(event.newIndex))].loader;
	mod("f_focusOnMe");
}

protected function mod(fungsi:String, param:String = ''):void
{
	if(m[idmod] != undefined) 
		if(m[idmod].child != null)
			if(m[idmod].child.hasOwnProperty(fungsi))
				if(param == '')
					m[idmod].child[fungsi]();
}

protected function MySuperTab_tabCloseHandler(event:SuperTabEvent):void
{
	closeForm(MySuperTab.getChildAt(event.tabIndex)['label'], m.ArrTab[CariIndexTab(MySuperTab.getChildAt(event.tabIndex)['label'], f_toolTipeMySuperTab(event.tabIndex))].loader);
	if(m.newTabIndex != event.tabIndex){
		if(m.newTabIndex < MySuperTab.numChildren)
			if(CariIndexTab(MySuperTab.getChildAt(m.newTabIndex)['label'], f_toolTipeMySuperTab(m.newTabIndex)) < m.ArrTab.length){
				idmod = m.ArrTab[CariIndexTab(MySuperTab.getChildAt(m.newTabIndex)['label'], f_toolTipeMySuperTab(m.newTabIndex))].loader;
				mod("f_focusOnMe");
			}
	}else{
		if(m.oldTabIndex < MySuperTab.numChildren)
			if(CariIndexTab(MySuperTab.getChildAt(m.oldTabIndex)['label'], f_toolTipeMySuperTab(m.oldTabIndex)) < m.ArrTab.length){
				idmod = m.ArrTab[CariIndexTab(MySuperTab.getChildAt(m.oldTabIndex)['label'], f_toolTipeMySuperTab(m.oldTabIndex))].loader;
				mod("f_focusOnMe");	
			}
	}
}

protected function f_tabCloseActive():void{
	if(MySuperTab.selectedIndex >= 0 && MySuperTab.getChildAt(MySuperTab.selectedIndex)['label'] != "Home"){
		if(MySuperTab.selectedIndex == MySuperTab.length-1)
			idmod = m.ArrTab[CariIndexTab(MySuperTab.getChildAt(MySuperTab.selectedIndex-1)['label'], f_toolTipeMySuperTab(MySuperTab.selectedIndex-1))].loader;
		else
			idmod = m.ArrTab[CariIndexTab(MySuperTab.getChildAt(MySuperTab.selectedIndex+1)['label'], f_toolTipeMySuperTab(MySuperTab.selectedIndex+1))].loader;
		closeForm(MySuperTab.getChildAt(MySuperTab.selectedIndex)['label'], m.ArrTab[CariIndexTab(MySuperTab.getChildAt(MySuperTab.selectedIndex)['label'], f_toolTipeMySuperTab(MySuperTab.selectedIndex))].loader);
		MySuperTab.removeChildAt(MySuperTab.selectedIndex);
		mod("f_focusOnMe");
	}
}

public function f_tabCloseAll():void{
	for(i=MySuperTab.length-1;i>=0;i--)
		if(MySuperTab.getChildAt(i)['label'] != "Home"){
			closeForm(MySuperTab.getChildAt(i)['label'], m.ArrTab[CariIndexTab(MySuperTab.getChildAt(i)['label'], f_toolTipeMySuperTab(i))].loader);
			MySuperTab.removeChildAt(i);
		}
}

public function f_createReport(Module:int, Menu:int, Item:int, Filter:String, OrderBy:String, GroupBy:String, Extension:int, Param:String, Title:String, RQuery:int = 1, Sumber:String = '', IDTransaksi:int = 0, watermark:String = ""):void
{
	m.s = F_splitArrayToString([WebAccessKey, Module, Menu, Item, Filter, OrderBy, GroupBy, RQuery, Extension, Param, userid, unama, IDTransaksi, Sumber, m.NamaPerusahaan, Title, m.PTKOTATTD, watermark, ubahasa]);
	var o:StiReport = new StiReport(m.url+sptLogin+m.s);
}

public function F_openReport(ModuleID:int, MenuID:int, ReportItem:int, Filter:String = "", Sort:String = "", GroupBy:String = "", FileExtention:int = 0, Param1:String = "", sumber:String = "", idtransaksi:int = 0, watermark:String = ""):void
{
	for(i=0;i<m.ArrReport.length;i++){
		ob = m.ArrReport[i];
		if(parseInt(ob.rmoduleid) == ModuleID && ob.rmenuid == MenuID && ob.ritem == ReportItem){
			f_createReport(ModuleID, MenuID, ob.ritem, Filter, Sort, GroupBy, FileExtention, Param1, ob.rtitle, ob.rquery, sumber, idtransaksi, watermark);
			return;
		}
	}
}

protected function openFormPopup(StrLabel:String, StrURL:String, Tinggi:int, Lebar:int, LevelPopUp:int=1):void 
{
	// set id 
	m.namaLoader = "popUpLoader"+LevelPopUp;
	m.namaWindow = "winPopUp"+LevelPopUp;
	m.mod[m.namaLoader] = {};
	
	// add pop up
	PopUpManager.addPopUp(m[m.namaWindow], this, true);
	
	// tampung id di variable 
	if(LevelPopUp == 1)
		m.modTemp[0] = idmod;
	idmodcurr = idmod;
	idmod = m.namaLoader;
	m.mod[idmod] = {};
	m.modTemp[LevelPopUp] = idmod;
	o = m[idmod].child;
	
	// set property loader
	m[m.namaLoader].height = Tinggi;
	m[m.namaLoader].width = Lebar;
	m[m.namaWindow].width = Lebar;
	m[m.namaWindow].height = Tinggi+30;
	m[m.namaWindow].title = StrLabel;
	m[m.namaLoader].loadModule(StrURL);
	
	// center pop up posisi
	PopUpManager.centerPopUp(m[m.namaWindow]);
}

public function openFormX(StrLabel:String, IsPopup:int, StrURL:String, Tinggi:int = 400, Lebar:int = 600, Tipe:String = 't1', Filter:String = '', Arrfilter:Array = null, lvlPopUp:int = 1):void {
	var noModul:int;
	
	FormFilter = Filter;
	m.ArrFilter = Arrfilter;
	
	if(IsPopup == 0){
		m.child = new Box();
		with(m.child)
		{
			setStyle("closable", true);
			setStyle("borderStyle","solid");
			setStyle("borderVisible",true);
			x = 0;
			y = 0;
			toolTip = StrLabel;
			label = StrLabel;
		}
		  
		//Close Form yg labelnya sama
		if(CekTab(StrLabel, Tipe) == false)
		{
			closeForm(StrLabel, m.ArrTab[CariIndexTab(StrLabel, Tipe)].loader);
			MySuperTab.removeChildAt(CariIndexTab(StrLabel, Tipe)); 
		}
		
		for(i=0;i<=20;i++)
			if(m['DynamicLoader'+i].url.length == 0){
				noModul = i;
				if(m.mod.hasOwnProperty('DynamicLoader'+noModul))
				{
					if(m.mod['DynamicLoader'+noModul].StrLabel != StrLabel)
					{
						m['DynamicLoader'+noModul] = new ComModuleLoader;
						m['DynamicLoader'+noModul].id = 'DynamicLoader'+noModul;
						m['DynamicLoader'+noModul].url = '';
					} 
				}
				m.mod['DynamicLoader'+noModul] = {};
				m.mod['DynamicLoader'+noModul].moduleid = StrURL.slice(5,6);
				m.mod['DynamicLoader'+noModul].FormFilter = Filter;
				m.mod['DynamicLoader'+noModul].ArrFilter = Arrfilter;
				m.mod['DynamicLoader'+noModul].StrLabel = StrLabel;
				m.mod['DynamicLoader'+noModul].IsPopup = IsPopup;
				m.mod['DynamicLoader'+noModul].StrURL = StrURL;
				m.mod['DynamicLoader'+noModul].Tinggi = Tinggi;
				m.mod['DynamicLoader'+noModul].Lebar = Lebar;
				m.mod['DynamicLoader'+noModul].Tipe = Tipe;
				
				with(m['DynamicLoader'+noModul])
				{
					x = 0;
					y = 0; 
					percentHeight=100;
					percentWidth=100;	
					loadModule(StrURL);
				}
				with(m.child)
				{
					addChild(m['DynamicLoader'+noModul]);
					icon=this[Tipe];
				}
				
				with(MySuperTab)
				{
					addChild(m.child);
					selectedIndex = MySuperTab.numChildren-1;
				}
				m.ArrTab.addItem({label:StrLabel, loader:'DynamicLoader'+noModul, tipe:Tipe, id:StrLabel+"_"+Tipe});
				break; 
			}
		
		if(MySuperTab.length>20) 
			Alert.show(l("Jumlah maksimal module yang bisa dibuka bersamaan adalah 20 saja"), l("Informasi"));
	}else if(IsPopup==1)
		openFormPopup(StrLabel, StrURL, Tinggi, Lebar, lvlPopUp);
}

public function CekTab(Label:String, Tipe:String):Boolean
{
	for(i=0;i<m.ArrTab.length;i++)
		if(m.ArrTab[i].label == Label && m.ArrTab[i].tipe == Tipe)
			return false;
	return true;
}

public function CariIndexTab(label:String, Tipe:String):int
{
	for(i=0;i<MySuperTab.length;i++)
		if(MySuperTab.getChildAt(i)['label'] == label && f_toolTipeMySuperTab(i) == Tipe)
			return i;
	return 0;
}

public function HapusTabdariArray(Label:String, Loader:String):void
{
	for(i=0;i<m.ArrTab.length;i++)
		if(m.ArrTab[i].label == Label && m.ArrTab[i].loader == Loader){
			m.ArrTab.removeItemAt(i);
			return;
		}
	Alert.show(l("Modul tidak terdefinisi"), l("Informasi"));
}

public function f_cariIdmod(Label:String, Tipe:String):String
{
	for(i=0;i<m.ArrTab.length;i++)
		if(m.ArrTab[i].label == Label && m.ArrTab[i].tipe == Tipe)
			return m.ArrTab[i].loader;
	return '';
}
 
public function closeForm(Label:String, Loader:String):void
{	
	for(j=0;j<m.ArrTab.length;j++)
		if(m.ArrTab[j].label == Label && m.ArrTab[j].loader == Loader){
			//kosongkanloader
			m[m.ArrTab[j].loader].url = '';
			m[m.ArrTab[j].loader].unloadModule();
			m.mod[m.ArrTab[j].loader] = {};
			//hapus dari array
			HapusTabdariArray(Label, Loader);
			return;
		}
	Alert.show(l("Modul tidak terdefinisi"), l("Informasi"));
}

public function focusMgr(backward:Boolean = false):void
{
	m.a = idmod.split("popUpLoader");
	if(m.a.length > 1){
		m["winPopUp"+m.a[1]].focusManager.getNextFocusManagerComponent(backward).setFocus();
	}else
		focusManager.getNextFocusManagerComponent(backward).setFocus();
}

protected function lblex_clickHandler(event:MouseEvent):void
{
	lblex.text = "";
}

public function aksesfromjs():void
{
	if(m.login)
		f_logout();
}

protected var childidx:int = 0;
protected function MySuperTab_childIndexChangeHandler(event:IndexChangedEvent):void
{
	if(childidx == 0){
		childidx += 1;
		if((event.newIndex-event.oldIndex) > 0){
			m.ArrTab.addItemAt(m.ArrTab.getItemAt(event.oldIndex), event.newIndex+1);
			m.ArrTab.removeItemAt(event.oldIndex);
		}else{
			m.ArrTab.addItemAt(m.ArrTab.getItemAt(event.oldIndex), event.newIndex);
			m.ArrTab.removeItemAt(event.oldIndex+1);
		}
	}else if(childidx == 1){
		childidx = 0;
	}
	
}

public function f_getReloginForm():void
{
	winRelogin = new ComReLogin;
	PopUpManager.addPopUp(winRelogin, this, true);
	winRelogin.txtUsername.text = ukode;
	PopUpManager.centerPopUp(winRelogin);
	winRelogin.txtPassword.setFocus();
}
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
	
	function detect(o:Object):void {
		var i:int;
		var x:int=o.dataProvider.length;
		var stat:Boolean=false;
		for(i=0;i<x;i++) {
			if(o.textInput.text==o.dataProvider.getItemAt(i).l) {
				stat = true;
			}	
		}
		if(stat==false)
		{
			o.textInput.text = "";
		}
		if(o.selectedIndex<0)
		{
			o.textInput.text = "";
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
	
	function detect(o:Object):void {
		var i:int;
		var x:int=o.dataProvider.length;
		var stat:Boolean=false;
		for(i=0;i<x;i++) {
			if(o.textInput.text==o.dataProvider.getItemAt(i).l) {
				stat = true;
			}	
		}
		if(stat==false)
		{
			o.textInput.text = "";
		}
		if(o.selectedIndex<0)
		{
			o.textInput.text = "";
		}
	}	
}

public function F_PilihTabBar(o:Object,ind:int):void
{
	o.selectedIndex = ind;
	o.validateNow();
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

//melakukan set pada textinput agar menjadi numberonly input POS
public function F_TextInputNumberOnlyModePOS(arTextInput:Array):void
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
		event.currentTarget.text = m.nfNumberPOS.format(event.currentTarget.text);
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
		m[idmod].child.o[event.currentTarget.id] = event.currentTarget.text.split(m.nfNominal.decimalSeparator).join('.');
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m.nfNominal.format(m[idmod].child.o[event.currentTarget.id]);
	}
}
//melakukan set pada textinput sempurna
public function F_TextInputMinusDesimal(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = m.nfNominal.decimalSeparator+"-/0-9";
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
		m[idmod].child.o[event.currentTarget.id] = event.currentTarget.text.split(m.nfNominal.decimalSeparator).join('.');
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m.nfNominal.format(m[idmod].child.o[event.currentTarget.id]);
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
public function F_TextInputNumberDesimalModePOS(arTextInput:Array):void
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
		m[idmod].child.o[event.currentTarget.id] = event.currentTarget.text.split(m.nfNominal.decimalSeparator).join('.');
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		event.currentTarget.text = m.nfNominalPOS.format(m[idmod].child.o[event.currentTarget.id]);
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
		var o:Object = event.currentTarget;
		var dt:String = o.text;
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
				default:o.text = '';break;
			}
		else if(d.length == 2)
			f_valid(d[0], d[1], dTglDefault[2]);
		else if(d.length == 3)
			f_valid(d[0], d[1], d[2]);
		else o.text = '';
		
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
			o.text = d1+"/"+d2+"/"+yyyy;
		}		
	}
	
}

// ------------------------------------------------------ 26 April 2013 Fatkur --------------------------------------------

// --------- Panggil WS  ---------------

public var wsValidasi:Boolean = true, wsParam:String = '';
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
public function F_wsUpload(Subject:String, paket:String, userid:String, filePaket:String, fileExtension:String, data:ByteArray, idtransaksi:String = "", namafile:String = '', catatan:String = '', ukuranfile:String = '', tanggal:String = '', pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", idtransaksi2:String = '', fdefault:int = 0):void
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
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, String(userId), isUpdate, s	);
	vg_master = "";
}

// WS Search
//public function F_wsSearch(Subject:String, paket:String, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = '', userid:String = "0"):void
//{
	//if(formatTgl == '')formatTgl = m.formatDate;
	//if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	//F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, userid)
//}
public function F_wsSearch(Subject:String, paket:String, pageNumber:int = 0, itemLimit:int = 0, strFilter:String = "", strSort:String = "", formatTgl:String = '', formatTglWaktu:String = '', userid:String = "0", Data:String=""):void
{
	if(formatTgl == '')formatTgl = m.formatDate;
	if(formatTglWaktu == '')formatTglWaktu = m.formatDate+" "+m.formatTime
	F_callWS(Subject, paket, pageNumber+sptSubParam+itemLimit+sptSubParam+strFilter+sptSubParam+strSort+sptSubParam+formatTgl+sptSubParam+formatTglWaktu, userid, true, Data)
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
public function f_refreshIDmodule(obj:Object = null):void
{
	if(obj == null){
		if(idmod.split('DynamicLoader').length > 1)
			idmod = m.ArrTab[MySuperTab.selectedIndex].loader;
	}else{
		m.a = String(obj).split('.');
		if(m.a[0] == 'MyERPPlus')
			module = m.a[7];
		else if(m.a[0].split('winPopUp')['length'] > 1)
			module = m.a[5];
		else module = idmod;
	}
}

public function F_txtpencarian(arr:Array):void{
	f_refreshIDmodule(arr[0]);
	o = m[module].child;
	for(i=0;i<arr.length;i++)
		add(arr[i])
	 
	function add(txt:Object):void{
		txt.addEventListener(FocusEvent.FOCUS_IN, f_focusin);
		txt.addEventListener(FocusEvent.FOCUS_OUT, f_focusout);  
		txt.addEventListener(KeyboardEvent.KEY_DOWN, f_keydown);
		
		o.o[txt.id+"cari"] = new Image();
		o.o[txt.id+"cari"].left = (txt.left+txt.width+3);
		if(txt.hasOwnProperty('top'))
		o.o[txt.id+"cari"].top = (txt.top+3); 
		else
		o.o[txt.id+"cari"].bottom = (txt.bottom+3); 
		o.o[txt.id+"cari"].source = tbfind; 
		f_cariklik(o.o[txt.id+"cari"], txt);
		txt.parent.addElement(o.o[txt.id+"cari"]); 
		
		function f_focusin(e:FocusEvent):void{
			vpIdTxt = txt;
			if(vpSetIn == true){
				vpVarCurrent = txt.text;
				vpSetIn = false;
			}
		}
		function f_focusout(e:FocusEvent):void{
			f_refreshIDmodule(txt);
			if(m.hasOwnProperty(module))
			{
				if(m[module].hasOwnProperty('child'))
				{
					o = m[module].child;
				}
				else
				{
					return;
				}
			}
			else 
			{
				return;
			}
			if(vpOnEnter == false)vpFocusEvent = false;
			if(txt.text == '' && txt.id != null && o != null){
				if(o.hasOwnProperty('f_kosongkanText'))o.f_kosongkanText(txt.id);
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
					}else if(vpVarCurrent == txt.text || txt.text.length == 0)
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
	f_refreshIDmodule(arr[0]);
	o = m[module].child;
	for(i=0;i<arr.length;i++)
		add(arr[i])
	
	function add(txt:Object):void{
		txt.addEventListener(KeyboardEvent.KEY_DOWN, f_keydown);
		
		o.o[txt.id+"cari"] = new Image();
		o.o[txt.id+"cari"].left = (txt.left+txt.width+3);
		o.o[txt.id+"cari"].top = (txt.top+3); 
		o.o[txt.id+"cari"].source = tbfind; 
		f_cariklik(o.o[txt.id+"cari"], txt);
		txt.parent.addElement(o.o[txt.id+"cari"]); 
		
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
	o = m[idmod].child;
	if(vpFocusEvent == false){
		o.f_kosongkanText(target);
	}else{
		if(!m.bukacompencarian || (idmodcurr != "" && !m.bukacompencarian2))
		{
			if(o.f_filter(target)[0] != '')
				F_popUpCari(target);
		}
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

public function F_validasiTrueWin():void{
	if(wsSuccess == true){
		if(vpFocusEnter == true)
			mod('focusMgr');
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
	f_refreshIDmodule(); 
	o = m[idmod].child;
	if(o.hasOwnProperty("f_filter")){
		f = o.f_filter(target);
		if(f[0] != ''){
			m.rowCari = rowIndex;
			if(f[0] == "CdM12_Item" ||f[0] == "CdM12_Contact" ){
				FlexGlobals.topLevelApplication.F_wsSearch(target, f[0],f[3], f[4], f[1], f[2], "", "", String(FlexGlobals.topLevelApplication.userid));
			}
			else if(f[0] == "CdM12_ItemCategoryPOS"){
				FlexGlobals.topLevelApplication.F_wsSearch(target, f[0], f[3], f[4], f[1], f[2], "", "", kategoriPOS);
			}
			else if(f[0] == "CdM1_Item" && txtGudang != "" && (m.mod[idmod].StrURL == "mod/m3/m3_TransaksiTS.swf" || m.mod[idmod].StrURL == "mod/m3/m3_TransaksiRS.swf" || m.mod[idmod].StrURL == "mod/m3/m3_TransaksiSP.swf" || m.mod[idmod].StrURL == "mod/m3/m3_TransaksiSA.swf" || m.mod[idmod].StrURL == "mod/m3/m3_TransaksiIB.swf" || m.mod[idmod].StrURL == "mod/m4/m4_TransaksiPO.swf" || m.mod[idmod].StrURL == "mod/m4/m4_TransaksiRI.swf" || m.mod[idmod].StrURL == "mod/m4/m4_TransaksiDNR.swf" || m.mod[idmod].StrURL == "mod/m4/m4_TransaksiPRT.swf" || m.mod[idmod].StrURL == "mod/m5/m5_TransaksiSO.swf" || m.mod[idmod].StrURL == "mod/m5/m5_TransaksiDO.swf" || m.mod[idmod].StrURL == "mod/m5/m5_TransaksiSI.swf" || m.mod[idmod].StrURL == "mod/m5/m5_TransaksiRNR.swf" || m.mod[idmod].StrURL == "mod/m5/m5_TransaksiSR.swf" || m.mod[idmod].StrURL == "mod/m6/m6_TransaksiPD.swf" )){
				F_wsSearch(target, f[0], f[3], f[4], f[1], f[2], "", "", String(FlexGlobals.topLevelApplication.userid), txtGudang);
			}
			else{
				F_wsSearch(target, f[0], f[3], f[4], f[1], f[2]);
			}
		}
	}
}

public function F_popUpCari(subject:String, kondisi:String = ''):void{
	f_refreshIDmodule();
	if(m[idmod].child.hasOwnProperty('f_filter')){
		var f:Array = m[idmod].child.f_filter(subject);
		if(f[0].length > 0)
			if(f[7])
			{
				if(f.length == 9)
					if(f[8].length == 0)
					{
						Alert.show(l("ID Primary Key Pencarian Tidak adak"), l("Informasi"));
						return;
					}
				if(f.length == 10)
				{
					F_cariDataCombo( f[0], f[5], f[2], subject, f[7], f[9], f[8]);
				}
				else
					F_cariDataCombo( f[0], f[5], f[2], subject, f[7], kondisi, f[8]);
			}
			else if(f.length == 10)
			{
				F_cariDataCombo( f[0], f[5], f[2], subject, f[7], f[9], f[8]);
			}
			else
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
	var n:int, j:int, x:int, o:Object, obj:Object, ob:Object = {};
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
					ob.skip_l = [];ob.type_l = [];ob.labf_l = [];ob.edit_l = [];
				}
				arr[n].columns = new ArrayList;
				arr[n].setStyle("alternatingRowColors", ['#FFFFFF', '#EFF3FA']);
				if(typeGrid == "POS")
				{
					arr[n].setStyle("alternatingRowColors", ['#202328']);
				}
				ob.skip = [];ob.type = [];ob.labf = [];ob.edit = [];
				for(x=0;x<wsJson.grid[j].property.length;x++){
					o = wsJson.grid[j].property[x];
					
					// Item Editor
					switch(int(o.ie)){
						case 0:ob.ie = undefined;break;
						case 1:ob.ie = new ClassFactory(ie_nominal);break;
						case 2:ob.ie = new ClassFactory(ie_date);break;
						case 3:ob.ie = new ClassFactory(ie_discount);break;
						case 4:ob.ie = new ClassFactory(ie_number);break;
						case 5:ob.ie = new ClassFactory(ie_numericStepper);break;
						case 6:ob.ie = new ClassFactory(ie_search);break;
						case 7:ob.ie = new ClassFactory(ie_comboBox);break;
						case 8:ob.ie = new ClassFactory(ie_custom);break;
						case 9:ob.ie = new ClassFactory(ie_searchBarcode);break;
						case 10:ob.ie = new ClassFactory(ie_dinamis);break; 
						case 11:ob.ie = new ClassFactory(ie_multiline);break;
						default:ob.ie = undefined;
					}
					
					// Label Function
					switch(int(o.lf)){
						case 0:ob.lf = F_formatDefaultString;break;
						case 1:ob.lf = F_formatCurreny;break;
						case 2:ob.lf = F_formatJml;break;
						case 3:ob.lf = F_formatDate;
							ob.sc = date_sortCompareFunc;
							break;
						case 4:ob.lf = F_format1Baris;break;
						case 5:ob.lf = F_formatDateTime;
							ob.sc = date_sortCompareFunc;
							break;
						case 6:ob.lf = F_formatCurrenyPOS;break;
						default:ob.lf = F_formatDefaultString;
					}
					
					// Header Renderer 
					switch(int(o.hr)){
						case 0:
							ob.hr = new ClassFactory(r_gridHeaderRenderer);
							ob.hr.properties = {headerTextAlign:"left"};
							break;
						case 1:
							ob.hr = new ClassFactory(r_gridHeaderRenderer);
							ob.hr.properties = {headerTextAlign:"center"};
							break;
						case 2:
							ob.hr = new ClassFactory(r_gridHeaderRenderer);
							ob.hr.properties = {headerTextAlign:"right"};
							break;
						case 3:ob.hr = new ClassFactory(ir_checkBoxHeader);break; 
						default:
							ob.hr = new ClassFactory(r_gridHeaderRenderer);
							ob.hr.properties = {headerTextAlign:"left"};;
					}
					
					// Item Renderer
					switch(int(o.ir)){
						case 0:
							ob.ir = new ClassFactory(r_gridItemRenderer);
							ob.ir.properties = {columnTextAlign:"left"};
							break;
						case 1:
							ob.ir = new ClassFactory(r_gridItemRenderer);
							ob.ir.properties = {columnTextAlign:"center"};
							break;
						case 2:
							ob.ir = new ClassFactory(r_gridItemRenderer);
							ob.ir.properties = {columnTextAlign:"right"};
							break;
						case 3:ob.ir = new ClassFactory(ir_checkBox);break;
						case 4:ob.ir = new ClassFactory(ir_comboBox);break;
						case 5:ob.ir = new ClassFactory(ir_link);break;
						case 6:ob.ir = new ClassFactory(ir_radioButton);break;
						case 7:ob.ir = new ClassFactory(ir_search);break;
						case 8:ob.ir = new ClassFactory(ir_date);break;
						case 9:ob.ir = new ClassFactory(ir_country);break;
						case 10:ob.ir = new ClassFactory(ir_yesno);break;
						case 11:ob.ir = new ClassFactory(ir_uploadFile);break;
						case 12:ob.ir = new ClassFactory(ir_config);break;
						case 13:ob.ir = new ClassFactory(ir_download);break;
						case 14:ob.ir = new ClassFactory(ir_custom);break;
						case 15:ob.ir = new ClassFactory(ir_master);break;
						case 16:ob.ir = new ClassFactory(ir_dinamis);break;
						case 17:ob.ir = new ClassFactory(ir_multiline);break;
						default:
							ob.ir = new ClassFactory(r_gridItemRenderer);
							ob.ir.properties = {columnTextAlign:"left"};
					}
					o.e = int(o.e);
					o.v = int(o.v); 
					if(x < lockedColumn && lockedColumn > 0){
						ob.edit_l[ob.edit_l.length] = int(o.e);
						ob.skip_l[ob.skip_l.length] = int(o.s);
						ob.type_l[ob.type_l.length] = o.r;
						ob.labf_l[ob.labf_l.length] = int(o.lf_l);
						obj.columns.addItem(F_tambahKolomGrid(o.w, o.df, l(o.ht), o.e, o.v, ob.lf, ob.hr, ob.ir, ob.ie, ob.sc));
					}else{
						ob.edit[ob.edit.length] = int(o.e);
						ob.skip[ob.skip.length] = int(o.s);
						ob.type[ob.type.length] = o.r;
						ob.labf[ob.labf.length] = int(o.lf);
						arr[n].columns.addItem(F_tambahKolomGrid(o.w, o.df, l(o.ht), o.e, o.v, ob.lf, ob.hr, ob.ir, ob.ie, ob.sc));
					}	
				}
				m.mod[module]["e"+arr[n].id] = ob.edit;
				m.mod[module]["s"+arr[n].id] = ob.skip;
				m.mod[module]["t"+arr[n].id] = ob.type;
				m.mod[module]["lf"+arr[n].id] = ob.labf;
				m.mod[module]["e_l"+arr[n].id] = ob.edit_l;
				m.mod[module]["s_l"+arr[n].id] = ob.skip_l;
				m.mod[module]["t_l"+arr[n].id] = ob.type_l;
				m.mod[module]["lf_l"+arr[n].id] = ob.labf_l;
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
						m.mod[module]["lf"+obj.id] = ob.labf_l;
						F_standartGrid((obj as DataGrid), ob.skip_l, ob.type_l, false);
						F_standartGrid(arr[n], ob.skip, ob.type, false);
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else
						F_standartGrid(arr[n], ob.skip, ob.type);
				}else if(typeGrid == 'TransaksiBarang'){
					if(lockedColumn > 0){
						if(data == ''){
							arr[n].dataProvider = m[module].child[arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[arr[n].id+"_ac"];
						}else{
							arr[n].dataProvider = m[module].child[data][arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[data][arr[n].id+"_ac"];
						}
						m.mod[module]["lf"+obj.id] = ob.labf_l;
						F_standartGrid((obj as DataGrid), ob.skip_l, ob.type_l, false);
						F_standartGrid(arr[n], ob.skip, ob.type, false);
						TransaksiBarang((obj as DataGrid));
						TransaksiBarang(arr[n]);
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else{
						F_standartGrid(arr[n], ob.skip, ob.type);
						TransaksiBarang(arr[n]);
					}
				}else if(typeGrid == 'TransaksiBarangBarcode'){
					if(lockedColumn > 0){
						if(data == ''){
							arr[n].dataProvider = m[module].child[arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[arr[n].id+"_ac"];
						}else{
							arr[n].dataProvider = m[module].child[data][arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[data][arr[n].id+"_ac"];
						}
						m.mod[module]["lf"+obj.id] = ob.labf_l;
						F_standartGridBarcode((obj as DataGrid), ob.skip_l, ob.type_l, false);
						F_standartGridBarcode(arr[n], ob.skip, ob.type, false);
						TransaksiBarang((obj as DataGrid));
						TransaksiBarang(arr[n]);
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else{
						F_standartGridBarcode(arr[n], ob.skip, ob.type);
						TransaksiBarang(arr[n]);
					}
				}
				else if(typeGrid == 'TransaksiBarangPOS'){
					if(lockedColumn > 0){
						if(data == ''){
							arr[n].dataProvider = m[module].child[arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[arr[n].id+"_ac"];
						}else{
							arr[n].dataProvider = m[module].child[data][arr[n].id+"_ac"];
							obj.dataProvider = m[module].child[data][arr[n].id+"_ac"];
						}
						m.mod[module]["lf"+obj.id] = ob.labf_l;
						F_standartGridPOS((obj as DataGrid), ob.skip_l, ob.type_l, false);
						F_standartGridPOS(arr[n], ob.skip, ob.type, false);
						TransaksiBarang((obj as DataGrid));
						TransaksiBarang(arr[n]);
						F_GridLockedColumn((obj as DataGrid), arr[n]);
					}else{
						F_standartGridPOS(arr[n], ob.skip, ob.type);
						TransaksiBarang(arr[n]);
					}
				}
				else if(typeGrid == 'data')
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
	
	function TransaksiBarang(dg:DataGrid):void
	{
		var vCMI:ContextMenuItem;
		vCMI = new ContextMenuItem(l('Tampilkan Gambar'))      
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgTampilkanGambar);  
		dg.contextMenu.customItems.push(vCMI);
		
		function f_dgTampilkanGambar():void
		{
			F_wsSearch(sptParam+"TampilkanGambar", "CdM1_Item", 0, 0, "bid = "+dg.dataProvider.getItemAt(dg.selectedCell.rowIndex).idbarang, "", "", "");
		}
	}
} 

public function F_language(data:String, type:int = 0):String
{
	if(data.length == 0)
		return "";
	if(m.bahasa == undefined)
		return data;
	for(i=0;i<m.bahasa.length;i++)
		if(m.bahasa[i].s == data){
			if(m.bahasa[i].t == '')
				return data;
			else
				return m.bahasa[i].t;
		}
	
	//if(m.languageSystemEnabled == false)
		//return data;
	if(m.SentenceSearch == undefined)
		return data;
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
  		resizableColumns = true;
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
  		resizableColumns = true;
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

public function F_standartGridBarcode(dg:DataGrid, skip:Array, type:Array, dp:Boolean = true):void
{
	var vCMI:ContextMenuItem;
	f_refreshIDmodule(dg);
	if(dp == true)
		dg.dataProvider = new ArrayCollection;
	with(dg){
		editable = true;
		sortableColumns = false;
  		resizableColumns = true;
		selectionMode = "singleCell";
	}
	rowIndex = 0;
	columnIndex = 0;
	//F_acAdd(dg)
	dg.dataProvider.addItem({no:dg.dataProviderLength+1});
	F_dgColumnFirst(dg);

	o = m[module].child; 
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
	vCMI = new ContextMenuItem(l('Insert'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgInsert); 
	
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Tambah'))      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgTambah); 
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Hapus Data'))      
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
		f_refreshIDmodule(dg);
		o = m[module].child;
		vpFocusEvent = true;
		if(dg.selectedCell == null)
			return;
		columnIndex = dg.selectedCell.columnIndex;
		if(rowIndex <= dg.dataProviderLength-1 && m.focusComboBox){
			if(vpVarCurrent != dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField]){
				if(o.hasOwnProperty("f_filter")){
					if(o.f_filter([dg.columns.getItemAt(columnIndex).dataField])[0] != 'CdM1_Transaction_Note_Detail')
					{
						F_cari(dg.columns.getItemAt(columnIndex).dataField);
					}
				}
				if(m.mod[module]['lf'+dg.id][columnIndex] == 1)
					dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField] = dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField].split(m.nfNominal.decimalSeparator)['join']('.');
				if(o.hasOwnProperty('f_grid'))o.f_grid(dg, dg.columns.getItemAt(columnIndex).dataField);
			}/*else f_skip(dg);*/
		}
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
					Alert.show(l('Delete baris ke-')+String(rowIndex+1)+' ?', l('Konfirmasi'), 3, null, function(e:Object):void{
						if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
						if(e.detail == Alert.YES){
							o = m[idmod].child;
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
							if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
							if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
						}
					});							
					break;
				case Keyboard.INSERT:
					F_insertDG(dg, rowIndex, skip);
					break;
				case Keyboard.F12:
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e_l"+dg.id]);
					if(m.b)
						F_popUpCari(dg.columns.getItemAt(columnIndex).dataField);
					break;
				case Keyboard.SPACE:
					if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'space');
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
				if(o.hasOwnProperty('f_filter')){
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e_l"+dg.id]);
					if(m.b && o.f_filter(m.column)[0] != '' && type[columnIndex] == 1 && (dg.dataProvider.getItemAt(rowIndex)[m.column] == null || dg.dataProvider.getItemAt(rowIndex)[m.column] == '' || dg.dataProvider.getItemAt(rowIndex)[m.column] == 0))
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
								vUp = false;
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
		o = m[idmod].child;
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
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
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
		if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
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
public function F_standartGridPOS(dg:DataGrid, skip:Array, type:Array, dp:Boolean = true):void
{
	var vCMI:ContextMenuItem;
	f_refreshIDmodule(dg);
	if(dp == true)
		dg.dataProvider = new ArrayCollection;
	with(dg){
		editable = true;
		sortableColumns = false;
		resizableColumns = true;
		selectionMode = "singleCell";
	}
	rowIndex = 0;
	columnIndex = 0;
	//F_acAdd(dg)
	dg.dataProvider.addItem({no:dg.dataProviderLength+1});
	F_dgColumnFirst(dg);
	
	o = m[module].child;
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
	vCMI = new ContextMenuItem(l('Insert'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgInsert); 
	
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Tambah'))      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgTambah); 
	if(int(getSettingPOS(0, 'app', 'LockDeleteItem')) == 0){
		dg.contextMenu.customItems.push(vCMI);
		vCMI = new ContextMenuItem(l('Hapus Data'))      
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgHapus);  
	}
	
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
		f_refreshIDmodule(dg);
		o = m[module].child;
		vpFocusEvent = true;
		if(dg.selectedCell == null)
			return;
		columnIndex = dg.selectedCell.columnIndex;
		if(rowIndex <= dg.dataProviderLength-1 && m.focusComboBox){
			if(vpVarCurrent != dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField]){
				if(o.hasOwnProperty("f_filter")){
					if(o.f_filter([dg.columns.getItemAt(columnIndex).dataField])[0] != 'CdM1_Transaction_Note_Detail'){
						F_cari(dg.columns.getItemAt(columnIndex).dataField);
					}
				}
				if(m.mod[module]['lf'+dg.id][columnIndex] == 1)
					dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField] = dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField].split(m.nfNominal.decimalSeparator)['join']('.');
				if(o.hasOwnProperty('f_grid'))o.f_grid(dg, dg.columns.getItemAt(columnIndex).dataField);
			}/*else f_skip(dg);*/
		}
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
					if(int(getSettingPOS(0, 'app', 'LockDeleteItem')) == 0){
						vgRowDel = rowIndex;
						Alert.show(l('Delete baris ke-')+String(rowIndex+1)+' ?', l('Konfirmasi'), 3, null, function(e:Object):void{
							if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
							if(e.detail == Alert.YES){
								o = m[idmod].child;
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
								if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
								if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
							}
						});		
					}				
					break;
				case Keyboard.INSERT:
					F_insertDG(dg, rowIndex, skip);
					break;
				case Keyboard.F12:
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e_l"+dg.id]);
					if(m.b)
						F_popUpCari(dg.columns.getItemAt(columnIndex).dataField);
					break;
				case Keyboard.SPACE:
					if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'space');
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
				if(o.hasOwnProperty('f_filter')){
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e_l"+dg.id]);
					if(m.b && o.f_filter(m.column)[0] != '' && type[columnIndex] == 1 && (dg.dataProvider.getItemAt(rowIndex)[m.column] == null || dg.dataProvider.getItemAt(rowIndex)[m.column] == '' || dg.dataProvider.getItemAt(rowIndex)[m.column] == 0))
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
								vUp = false;
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
		o = m[idmod].child;
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
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
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
		if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
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

public function F_standartGrid(dg:DataGrid, skip:Array, type:Array, dp:Boolean = true):void
{
	var vCMI:ContextMenuItem;
	f_refreshIDmodule(dg);
	if(dp == true)
		dg.dataProvider = new ArrayCollection;
	with(dg){
		editable = true;
		sortableColumns = false;
  		resizableColumns = true;
		selectionMode = "singleCell";
	}
	rowIndex = 0;
	columnIndex = 0;
	//F_acAdd(dg)
	dg.dataProvider.addItem({no:dg.dataProviderLength+1});
	F_dgColumnFirst(dg);

	o = m[module].child;
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
	vCMI = new ContextMenuItem(l('Insert'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgInsert); 
	
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Tambah'))      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_dgTambah); 
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Hapus Data'))      
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
		f_refreshIDmodule(dg);
		o = m[module].child;
		vpFocusEvent = true;
		if(dg.selectedCell == null)
			return;
		columnIndex = dg.selectedCell.columnIndex;
		if(rowIndex <= dg.dataProviderLength-1 && m.focusComboBox){
			if(vpVarCurrent != dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField]){
				if(o.hasOwnProperty("f_filter")){
					if(o.f_filter([dg.columns.getItemAt(columnIndex).dataField])[0] != 'CdM1_Transaction_Note_Detail'){
						F_cari(dg.columns.getItemAt(columnIndex).dataField);
					}
				}
				if(m.mod[module]['lf'+dg.id][columnIndex] == 1)
					dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField] = dg.dataProvider[rowIndex][dg.columns.getItemAt(columnIndex).dataField].split(m.nfNominal.decimalSeparator)['join']('.');
				if(o.hasOwnProperty('f_grid'))o.f_grid(dg, dg.columns.getItemAt(columnIndex).dataField);
			}/*else f_skip(dg);*/
		}
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
					Alert.show(l('Delete baris ke-')+String(rowIndex+1)+' ?', l('Konfirmasi'), 3, null, function(e:Object):void{
						if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
						if(e.detail == Alert.YES){
							o = m[idmod].child;
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
							if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
							if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
						}
					});							
					break;
				case Keyboard.INSERT:
					F_insertDG(dg, rowIndex, skip);
					break;
				case Keyboard.F12:
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[module]["e_l"+dg.id]);
					if(m.b)
						F_popUpCari(dg.columns.getItemAt(columnIndex).dataField);
					break;
				case Keyboard.SPACE:
					if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'space');
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
				if(o.hasOwnProperty('f_filter')){
					if(dg.id.split("_lock").length == 1)
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e"+dg.id]);
					else
						m.b = kelasku.KondisiEditGrid(dg, m.mod[idmod]["e_l"+dg.id]);
					if(m.b && o.f_filter(m.column)[0] != '' && type[columnIndex] == 1 && (dg.dataProvider.getItemAt(rowIndex)[m.column] == null || dg.dataProvider.getItemAt(rowIndex)[m.column] == '' || dg.dataProvider.getItemAt(rowIndex)[m.column] == 0))
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
								vUp = false;
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
		o = m[idmod].child;
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'pre_del');
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
		if(o.hasOwnProperty('f_grid'))o.f_grid(dg, 'del');
		if(o.hasOwnProperty('f_hitungTotal'))o.f_hitungTotal();
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
	var u:int;
	if(type == null){
		f_refreshIDmodule(dg);
		type = m.mod[module]['t'+dg.id];
	}
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
//		// Jika dison Bertingkat 
//		m.s = m.nfNominal.format(m.temp[0]);
//		for(i=1;i<m.temp.length;i++)
//			m.s += "+"+m.nfNominal.format(m.temp[i]);
		return item[column.dataField];
	}else
		return m.nfNominal.format(item[column.dataField]);
}

public function F_formatCurrenyPOS(item:Object, column:GridColumn):String
{
	if(item[column.dataField] == null || item[column.dataField] == '' || item[column.dataField] == undefined)
		item[column.dataField] = 0;
	
	item[column.dataField] = String(item[column.dataField]).split(m.nfNominalPOS.decimalSeparator).join('.');
	m.temp = item[column.dataField].split('+');
	if(m.temp.length > 1){
		//		// Jika dison Bertingkat 
		//		m.s = m.nfNominal.format(m.temp[0]);
		//		for(i=1;i<m.temp.length;i++)
		//			m.s += "+"+m.nfNominal.format(m.temp[i]);
		return item[column.dataField];
	}else
		return m.nfNominalPOS.format(item[column.dataField]);
}

public function F_formatNominal(data:String):String
{
	if(data == null || data == '')
		data = '0';
	
	data = data.split(m.nfNominal.decimalSeparator).join('.');
	m.temp = data.split('+');
	if(m.temp.length > 1){
//		// Jika dison Bertingkat 
//		m.s = m.nfNominal.format(m.temp[0]);
//		for(i=1;i<m.temp.length;i++)
//			m.s += "+"+m.nfNominal.format(m.temp[i]);
		return data;
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
	o = m[module].child;
	dg = m[module].child.dg;
	m.sort[module] = "";
	m.a = [];
	m.a[0] = m[module].child.a[0];
	m.a[1] = m[module].child.a[1];
	m.s = m[module].child.a[3];
	sumber = m[module].child.a[3].toLowerCase();
	mb = m[module].child.mb;
	o.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	// Config Paging
	o.txtpaging.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_pagingKEY_DOWN);
	o.txtpaging.restrict = "0-9";
	o.txtpaging.setStyle("textAlign","center");
	o.txtpaging.text = 0;
	o.txtpaging.maxChars = 4;
	o.imgfirst.enabled = false;
	o.imgprevious.enabled = false;
	o.imgnext.enabled = false;
	o.imglast.enabled = false;
	o.imgfirst.addEventListener(MouseEvent.CLICK, f_firstCLICK);
	o.imgprevious.addEventListener(MouseEvent.CLICK, f_previousCLICK);
	o.imgnext.addEventListener(MouseEvent.CLICK, f_nextCLICK);
	o.imglast.addEventListener(MouseEvent.CLICK, f_lastCLICK);
	
	function f_KEY_DOWN(e:KeyboardEvent):void
	{ 
		var id:int
		if(e.ctrlKey)
			switch(e.keyCode){
				case Keyboard.N: // Form Baru
					if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 1) == true){
						f_refreshIDmodule(obj);
						for(i=0;i<m.ArrUserMenu.length;i++)
							if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
								m.o = m.ArrUserMenu.getItemAt(i);
								break;
							}
						openFormX(m.o.mnname, 0, m.o.mnurl, 0, 0, "t2");
					}
					break;
				case Keyboard.E: // Edit Form
					if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 2) == true){
						if(dg.selectedCell != null){
							id = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
							f_refreshIDmodule(dg);
							if(apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 2))
								openForm(m[module].child.a[0], m[module].child.a[1], String(id));
						}
					}
					break;
				case Keyboard.D: // Delete Transaksi
					if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 3) == true){
						f_refreshIDmodule(dg);
						var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
						id = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
						if(dg.dataProviderLength > 0 && apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 3) == true)
							if(status == 0 || status == 5 || status == 6)
								Alert.show(l("Apakah Anda yakin akan menghapus No. Transaksi ini ?", 1), l("Informasi"), 3, null, function(e:Object):void{	
									if(e.detail == Alert.YES)
										F_wsDelete(sptParam+'updateStatus', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'Delete', userid, id.toString(), int(o.txtpaging.text), 20, o.f_filterdata(), m.sort[module]);
								});
							else Alert.show(l('Status Draft, Revisi dan Reject yang bisa di hapus', 1), l("Informasi"));
					}
					break;
				case Keyboard.P: // Cetak Form
					f_refreshIDmodule(dg);
					if(m[module].child.cmbjenislaporan.selectedIndex >= 0)
					F_openReport(m[module].child.a[0], m[module].child.a[1], m[module].child.cmbjenislaporan.selectedItem.ritem, m[module].child.f_filterdata());
					break;
				case Keyboard.R: // Reset Form
					o.f_kondisiAwal();
					break;
			}
		else 
			switch(e.keyCode){
				case Keyboard.HOME:
					if(int(o.txtpaging.text)>1)
						f_formDataTampil();
					break;
				case Keyboard.PAGE_UP:
					if(int(o.txtpaging.text)-1>0)
						f_formDataTampil(int(o.txtpaging.text)-1);
					break;
				case Keyboard.PAGE_DOWN:
					f_formDataTampil(int(o.txtpaging.text)+1);
					break;
				case Keyboard.END:
					f_formDataTampil(-1);
					break;
				case Keyboard.F5:
					f_refreshIDmodule(dg);
					if(int(m[module].child.txtpaging.text) == 0)
						f_formDataTampil(1);
					else
						f_formDataTampil(int(m[module].child.txtpaging.text)); 
					break;
		}
	}
	
	
	function f_firstCLICK():void
	{
		f_refreshIDmodule(dg);
		if(m[module].child.imgfirst.enabled)
			f_formDataTampil();
	}
	
	function f_previousCLICK():void
	{
		f_refreshIDmodule(dg)
		if(m[module].child.imgprevious.enabled)
			f_formDataTampil(int(m[module].child.txtpaging.text)-1);
	}
	
	function f_nextCLICK():void
	{
		f_refreshIDmodule(dg)
		if(m[module].child.imgnext.enabled)
			f_formDataTampil(int(m[module].child.txtpaging.text)+1);
	}
	
	function f_lastCLICK():void
	{
		f_refreshIDmodule(dg)
		if(m[module].child.imglast.enabled)
			f_formDataTampil(int(-1));
	}
	
	function f_pagingKEY_DOWN(e:KeyboardEvent):void
	{
		f_refreshIDmodule(dg)
		if(e.keyCode == Keyboard.ENTER)
			if(int(m[module].child.txtpaging.text)>0)
				f_formDataTampil(int(m[module].child.txtpaging.text));
	}
	
	for(i=0;i<m.ArrUserMenu.length;i++)
		if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
			m.o = m.ArrUserMenu.getItemAt(i);
			break;
		}
	
	if(m[module].child.a[4]){
		m[module].child.cmbstatus.dataProvider =  new ArrayCollection([{l:'Semua', v:-1}, {l:'Aktif', v:15}, {l:'Draft', v:0}, {l:'Need Approval', v:1}, {l:'Approved', v:2}, {l:'In Progress', v:3}, {l:'Complete', v:4}, {l:'Revisi', v:5}, {l:'Reject', v:6}, {l:'Close', v:7}, {l:'Approve 1', v:8}, {l:'Approve 2', v:9}, {l:'Approve 3', v:10}, {l:'Approve 4', v:11}]);
	}else{
		m[module].child.cmbstatus.dataProvider =  new ArrayCollection([{l:'Semua', v:-1}, {l:'Aktif', v:15}, {l:'Draft', v:0}, {l:'Approved', v:2}, {l:'In Progress', v:3}, {l:'Complete', v:4}, {l:'Close', v:7}]);
	}
	m[module].child.txtlokasi.text = ulokasi;
	
	// Config Pop Up
//	F_standartTransaksiTerkait(m[module].child.gridjv);
//	F_standartTransaksiTerkait(m[module].child.gridtts);
//	F_standartTransaksiTerkait(m[module].child.gridtt);
//	m[module].child.gridjv.dataProvider = new ArrayCollection();
//	m[module].child.gridtts.dataProvider = new ArrayCollection();
//	m[module].child.gridtt.dataProvider = new ArrayCollection();
//	m[module].child.ttwpopup.addEventListener(mx.events.CloseEvent.CLOSE, f_ttwClose);
//	m[module].child.ttwpopup.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_ttwKEY_DOWN);
	
	// Config Grid
	dg.dataProvider = new ArrayCollection();
	dg.addEventListener(GridEvent.GRID_ROLL_OVER, f_dgROLL_OVER);
	dg.addEventListener(GridSortEvent.SORT_CHANGING, f_dgSort_changing);
	dg.selectionMode = "singleCell";
	dg.contextMenu = new ContextMenu;
	dg.contextMenu.hideBuiltInItems();
	dg.contextMenu.addEventListener(ContextMenuEvent.MENU_SELECT, f_MENU_SELECT);
	vCMI = new ContextMenuItem(l('Cetak Transaksi ini'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_cetakDetail);
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Transaksi Terkait'))
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_transaksiTerkait); 
	dg.contextMenu.customItems.push(vCMI);
	vCMI = new ContextMenuItem(l('Jurnal Voucher'))      
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jurnalVocher); 
	dg.contextMenu.customItems.push(vCMI);
	if(sumber != "pa" && sumber != "ppa"){
		vCMI = new ContextMenuItem(l('Jadikan Draft'))      
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jadikanDraft);  
		dg.contextMenu.customItems.push(vCMI);   
	}
	vCMI = new ContextMenuItem(l('Jadikan Close / Unclose'))     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_jadikanCloseUnclose);  
	dg.contextMenu.customItems.push(vCMI); 

	vCMI = new ContextMenuItem(l('History Transaksi'))     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_history);  
	dg.contextMenu.customItems.push(vCMI); 
	
	if(sumber != "pa"){
		vCMI = new ContextMenuItem(l('Hapus Transaksi'))     
		vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_hapus);  
		dg.contextMenu.customItems.push(vCMI);     
	}  
	// Config Menu Bar
	var arrMenuBar:ArrayCollection = new ArrayCollection([
		{label:"Tampilkan", icon:"tbpreview"},
		{label:"Reset", icon:"tbreset"},
		{label:"Tambah", icon:"tbadd"},
		{label:"Cetak", icon:"tbprint"},
		{label:"Export", icon:"tbexport", children:[
			{label:"Ms.Excel", icon:"tbexcel", data:"1"},
			{label:"Html", icon:"tbhtml", data:"2"},
			{label:"Ms.Word", icon:"tbword", data:"3"},
			{label:"Text", icon:"tbtxt", data:"4"},
			{label:"Image", icon:"tbtif", data:"5"},
			{label:"Excel Data", icon:"tbexcel", data:"6"}]
		},
		{label:"Lain-lain", icon:"tbothers", children:[
//			{label:"Bantuan", icon:"tbhelp"},
			{label:"Edit", icon:"tbedit"},
			{label:"Hapus", icon:"tbdelete"},
			{label:"Cetak Detail", icon:"tbprint"},
			{label:"Transaksi Terkait", icon:"tbrelated"},
			{label:"Jurnal Voucher", icon:"tbjurnal"},
			{label:"Jadikan Draft", icon:"tbdraft"},
			{label:"Jadikan Close/Unclose", icon:"tbclose_unclose"},
			{label:"Histori Transaksi", icon:"tbhistory"}]
		}]);
	F_settingmb(mb, arrMenuBar);
	mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, f_tampilkan); // tampil
	mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, f_reset); // reset
	mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, f_tambah); // tambah
	mb.menuBarItems[3].addEventListener(MouseEvent.CLICK, f_cetak); // cetak
	mb.addEventListener(mx.events.MenuEvent.ITEM_CLICK, f_mbItemClick); 
	function f_ttwClose():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		o.grppopup.visible = false;
		o.gridjv.visible = false;
		o.gridterkait.visible = false;
		o.gridjv.dataProvider.removeAll();
		o.gridtts.dataProvider.removeAll();
		o.gridtt.dataProvider.removeAll();
		o.dg.setFocus();
	}
	
	function f_ttwKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.keyCode == Keyboard.ESCAPE)
			f_ttwClose();
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
		var ob:Object = o.dg.columns.getItemAt(int(e.columnIndices)).dataField;
		if(vSort == true){
			vSort = false;
			f_formDataTampil(1, String(ob));
		}else if(vPosisi != int(e.columnIndices)){
			vSort = false;
			f_formDataTampil(1, String(ob));
		}else{
			vSort = true;
			dg.dataProvider.removeAll();
			f_formDataTampil(1, ob+' DESC');
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
		f_refreshIDmodule(dg);
//		if(int(m[module].child.txtpaging.text) == 0)
			f_formDataTampil(1);
//		else
//			f_formDataTampil(int(m[module].child.txtpaging.text));
	}
	
	
	function f_hapus():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 3) == true){
			f_refreshIDmodule(dg);
			sumber = m[module].child.a[3].toLowerCase();
			var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
			var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
			if(dg.dataProviderLength > 0 && apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 3) == true)
				if(status == 0 || status == 5 || status == 6)
					Alert.show(l("Apakah Anda yakin akan menghapus No. Transaksi ini ?", 1), l("Informasi"), 3, null, function(e:Object):void{	
						if(e.detail == Alert.YES)
							F_wsDelete(sptParam+'updateStatus', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'Delete', userid, id.toString(), int(o.txtpaging.text), 20, o.f_filterdata(), m.sort[module]);
					});
				else Alert.show(l('Status Draft, Revisi dan Reject yang bisa di hapus'), l("Informasi"));
		}
	}
	
	function f_reset():void
	{
		f_refreshIDmodule(dg);
		sumber = m[module].child.a[3].toLowerCase();
		m[module].child.f_kondisiAwal();
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
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 2) == true){
			if(dg.selectedCell != null){
				f_refreshIDmodule(dg);
				sumber = m[module].child.a[3].toLowerCase();
				var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
				if(apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 4) == true){
					f_refreshIDmodule(obj);
					for(i=0;i<m.ArrUserMenu.length;i++)
						if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
							m.o = m.ArrUserMenu.getItemAt(i);
							break;
						}
					openFormX(m.o.mnname, 0, m.o.mnurl, 0, 0, "t2", String(id));
				}
			} 
		}
	}
	
	function f_history():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 12) == true){
			if(dg.selectedCell != null){
				var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
				f_refreshIDmodule(dg);
				if(apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 4) == true){
					f_refreshIDmodule(obj);
					for(i=0;i<m.ArrUserMenu.length;i++)
						if(parseInt(m.ArrUserMenu.getItemAt(i).mnmoduleid) == m[module].child.a[0] && parseInt(m.ArrUserMenu.getItemAt(i).mnid) == m[module].child.a[1]){
							m.o = m.ArrUserMenu.getItemAt(i);
							break;
						}
					openFormX ("History "+m.o.mnname, 0, "mod/m0/m0_HistoryTransaksi.swf", 0, 0, "t3", String(id),[m[module].child.a[3], m[module].child.a[0], m[module].child.a[2]]);
				}
			} 
		}
	}
	
	function f_cetak():void
	{
		f_refreshIDmodule(dg);
		m.o = m[module].child
		sumber = m.o.a[3].toUpperCase();
		m.p = F_paramDate(m.o.dttgl1.text, m.o.dttgl2.text);
		if(m.o.cmbjenislaporan.selectedIndex >= 0){
			m.wmk = "";
			if(m[module].child.cmbstatus.selectedIndex >= 0)
				if(m[module].child.cmbstatus.selectedItem.v == 0)
					m.wmk = "Draft";	
			F_openReport(m.o.a[0], m.o.a[1], m.o.cmbjenislaporan.selectedItem.ritem, m.o.f_filterdata(), '', '', 0, m.p, sumber, 0);
		}else{
			m.o.cmbjenislaporan.setFocus();
			Alert.show(l("Pilih Jenis Laporan dulu"), l("Informasi"));
		}	
	} 
	
	function f_bantuan():void
	{
		f_refreshIDmodule(dg);
		F_help(m[module].child.a[0], m[module].child.a[1]);
	}
	
	function f_cetakDetail():void
	{ 
		f_refreshIDmodule(dg);
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
			m.wm = "";
			if(dg.dataProvider[dg.selectedCell.rowIndex][sumber+"status"] == 0)m.wm = "Draft";
			F_openReport(m.o.a[0], m.o.a[1], m.o.cmbjenislaporan.selectedItem.ritem, m.s, "", '', 0, m.p, sumber.toUpperCase(), dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"], m.wm);
		}else{
			m.o.cmbjenislaporan.setFocus();
			Alert.show(l("Pilih Jenis Laporan dulu"), l("Informasi"));
		}
	}
	
	function f_transaksiTerkait():void
	{
		f_refreshIDmodule(dg);
		sumber = m[module].child.a[3].toLowerCase();
		if(dg.dataProviderLength > 0)
			F_wsGetDataById(sptParam+'tampilFDTransaksiTerkait', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'Terkait', dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"]); 
	}
	
	function f_jurnalVocher():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 11) == true){
			f_refreshIDmodule(dg);
			sumber = m[module].child.a[3].toLowerCase();
			if(dg.selectedCell == null){
				Alert.show(l("Pilih data dulu"), l("Informasi"));
				return;
			} 
			var notransaksi:String = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"notransaksi"];
			if(notransaksi == null){
				Alert.show(l("Pilih No Transaksi dulu"), l("Informasi"));
				return;
			}
			if(dg.dataProviderLength > 0 && apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 11) == true)
				F_wsSearch(sptParam+'tampilFDJurnalVocher', 'M2_Transaction_Journal_VoucherSearch', 0, 0, "tnotransaksi = '"+notransaksi+"'", "tgrup, turutan");
		}
	}
	
	function f_jadikanDraft():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[2], 2) == true){
			f_refreshIDmodule(dg);
			sumber = m[module].child.a[3].toLowerCase();
			var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
			var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
			if(dg.dataProviderLength > 0)
				if(apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 2) == true)
					if(status == 2)
						F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'UpdateStatus', userid, id, '0', false, int(m[module].child.txtpaging.text), 20, m[module].child.f_filterdata(), m.sort[module]);
					else Alert.show(l('Hanya status approved yang bisa di jadikan Draft'), l('Informasi'));
		}
	}
	 
	function f_jadikanCloseUnclose():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[2], 10) == true){
			f_refreshIDmodule(dg);
			sumber = m[module].child.a[3].toLowerCase();
			var status:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"status"];
			var id:int = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex)[sumber+"id"];
			if(dg.dataProviderLength > 0 && apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 10) == true)
				if(status == 7)
					F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'UpdateStatus', userid, id, 'unclose', false, int(m[module].child.txtpaging.text), 20, m[module].child.f_filterdata(), m.sort[module]);
				else if(status == 2 || status == 3 || status == 4)
					F_wsUpdateStatus(sptParam+'updateStatus', 'M'+m[module].child.a[0]+'_'+m[module].child.a[3]+'UpdateStatus', userid, id, '7', false, int(m[module].child.txtpaging.text), 20, m[module].child.f_filterdata(), m.sort[module]);
				else Alert.show(l('Status Approved, Inprogress dan Complete yang bisa di Close'), l('Informasi'));	
		}
	}
	
	function f_historiTransaksi():void
	{
		if(apaBisaAkses(m[idmod].child.a[0], m[idmod].child.a[1], 12) == true){
			f_refreshIDmodule(dg);
			if(dg.dataProviderLength > 0 && apaBisaAkses(m[module].child.a[0], m[module].child.a[1], 12) == true)
				Alert.show(l('Form Histori Transaksi belum ada'), l('Informasi'));
		}
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
			case 'Histori Transaksi':f_history();
				break;
			case "Ms.Excel":case "Html": case "Ms.Word":case "Text":case "Image":
				f_refreshIDmodule(dg);
				m.o = m[module].child
				sumber = m.o.a[3].toUpperCase();
				m.p = F_paramDate(m.o.dttgl1.text, m.o.dttgl2.text);
				if(m.o.cmbjenislaporan.selectedIndex >= 0){
					m.wmk = "";
					if(m[module].child.cmbstatus.selectedIndex >= 0)
						if(m[module].child.cmbstatus.selectedItem.v == 0)
							m.wmk = "Draft";	
					F_openReport(m.o.a[0], m.o.a[1], m.o.cmbjenislaporan.selectedItem.ritem, m.o.f_filterdata(), '', '', e.item.data, m.p, sumber, 0);
				}else{
					m.o.cmbjenislaporan.setFocus();
					Alert.show(l("Pilih Jenis Laporan dulu"), l("Informasi"));
				}
		}
	}
}

public function f_formDataTampil(page:int = 1, sort:String = ''):void
{
	f_refreshIDmodule();
	o = m[idmod].child
	if(apaBisaAkses(o.a[0], o.a[1], 4) == true){
		if(sort == '')sort = o.a[3]+"inputtgl desc";
		m.sort[idmod] = sort;
		m.sumber = o.a[3].toLowerCase();
		F_wsSearch("★tampilForm", 'M'+o.a[0]+'_'+o.a[3]+'Search', page, 20, o.f_filterdata(), sort);
	}
}

// Form Master
public var idmodcurr:String;

public function f_reloadReset():void
{
	var data:Object = m.mod[idmod];
	FormFilter = "";
	m.ArrFilter = [];
	with(m[idmod]){
		x = 0;
		y = 0; 
		percentHeight=100;
		percentWidth=100;	
		unloadModule();
		loadModule();
	}
}

public function F_configMasterInput():void
{
	var arrMenuBar:ArrayCollection = new ArrayCollection([
		{label:"Simpan Baru", icon:"tbsavenew"},
		{label:"Simpan", icon:"tbsave"},
		{label:"Batal", icon:"tbclose"}]);
	F_settingmb(m[idmod].child.mb, arrMenuBar);
	o = m[idmod].child;
	o.txtkode.setFocus();
	o.vUserId = userid;
	o.vDefDt = DefaultTanggalforDB;
//	o.arr = ArrFilter;
	o.addEventListener(KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	o.mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, function():void{if(o.mb.menuBarItems[0].enabled)o.f_simpan(true)}); // simpanbaru
	o.mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, function():void{if(o.mb.menuBarItems[1].enabled)o.f_simpan(false)}); // simpan
	o.mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, function():void{closeFormPopUp(1)}); // tutup
	
	function f_KEY_DOWN(e:KeyboardEvent):void{
		if(e.ctrlKey && e.keyCode == Keyboard.N){if(o.mb.menuBarItems[0].enabled)o.f_simpan(true);
		}else if(e.ctrlKey && e.keyCode == Keyboard.S){if(o.mb.menuBarItems[0].enabled)o.f_simpan(false);
		}else if (e.keyCode == Keyboard.ESCAPE){closeFormPopUp(1)}
	}
}

public var vfplus:String='';
public function F_txtInOut(param:Object,fplus:String,paket:String):void
{
	o = m[idmod].child;
	o.vCurr = 'ῶ';
	param.addEventListener(FocusEvent.FOCUS_OUT, f_FOCUS_OUT);
	param.addEventListener(FocusEvent.FOCUS_IN, f_FOCUS_IN);
	
	function f_FOCUS_IN():void
	{
		if(o.vCurr != ''){
//			if (fplus == '') 
				o.vCurr = param.text;
			o.vKeyOut=true;
		}
	}
	
	function f_FOCUS_OUT():void
	{
		if(o.hasOwnProperty('vKeyOut'))
			if (o.vKeyOut==true){
				if (o.vCurr != param.text && o.isUpdate == false)
					if (fplus==''){
						vfplus='';
						F_wsDelete('cekid',paket,o.vUserId,param.text);
					}else
						F_wsDelete('cekid',paket,o.vUserId, vfplus);
				o.vKeyOut = false;
			}else o.vKeyOut = true;
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
		//Alert.show(ing.slice(0, ing.length-2)+' must be filled ('+ind.slice(0, ind.length-2)+' harus diisi dengan benar)');
		Alert.show(ind.slice(0, ind.length-2)+' harus diisi dengan benar', 'Informasi');
		return false;
	}
}

public function F_configMasterInputHasil():void
{
	if(wsSuccess == true){
		switch(subject){
			case 'simpan':
				closeFormPopUp(1);
				if(m[idmod].child.hasOwnProperty("dg"))
				{
					if(wsSuccess == true){
						m[idmod].child.dg.dataProvider = wsArrUtama;
						m[idmod].child.dg.setSelectedCell(0,0);
						rowIndex = 0;
						m[idmod].child.txtpaging.text = wsCurPage;
						m[idmod].child.imgfirst.enabled = wsIsPrev;
						m[idmod].child.imgprevious.enabled = wsIsPrev;
						m[idmod].child.imgnext.enabled = wsIsNext;
						m[idmod].child.imglast.enabled = wsIsNext;
					}else m[idmod].child.dg.dataProvider.removeAll();
				}
				else if(m.bukacompencarian)
				{ 
					//Tampilkan yg di tambahkan saja
					for(i=0;i<wsArrUtama.length;i++)
					{
						if(wsArrUtama[i][m.P1_field] == m.P1 && wsArrUtama[i][m.P2_field] == m.P2)
						{
							m.obj = wsArrUtama[i];
							wsArrUtama.removeAll();
							wsArrUtama.addItem(m.obj);
							winC.dg.dataProvider.removeAll();
							winC.txtfilter.text = m.P2;
							winC.f_persiapan(); 
							this.callLater(function():void{
								winC.f_pilih();
							});
							break;
						}
					}
				}
				break;
			case 'simpan_baru':
				if(m[idmodcurr].child.hasOwnProperty("dg"))
				{
					if(wsSuccess == true){
						m[idmodcurr].child.dg.dataProvider = wsArrUtama;
						m[idmodcurr].child.dg.setSelectedCell(0,0);
						rowIndex = 0;
						m[idmodcurr].child.txtpaging.text = wsCurPage;
						m[idmodcurr].child.imgfirst.enabled = wsIsPrev;
						m[idmodcurr].child.imgprevious.enabled = wsIsPrev;
						m[idmodcurr].child.imgnext.enabled = wsIsNext;
						m[idmodcurr].child.imglast.enabled = wsIsNext;
					}else m[idmodcurr].child.dg.dataProvider.removeAll();
				}
				else if(m.bukacompencarian)
				{ 
					//Tampilkan yg di tambahkan saja
					for(i=0;i<wsArrUtama.length;i++)
					{
						if(wsArrUtama[i][m.P1_field] == m.P1 && wsArrUtama[i][m.P2_field] == m.P2)
						{
							m.obj = wsArrUtama[i];
							wsArrUtama.removeAll();
							wsArrUtama.addItem(m.obj);
							winC.dg.dataProvider.removeAll();
							winC.txtfilter.text = m.P2;
							winC.f_persiapan(); 
							break;
						}
					}
				}
				o.txtkode.enabled =true;
				o.txtkode.setFocus();
				break;
			case 'cekid':o.vCurr = 'ῶ';break;
		}
	}else{
		switch(subject){
			case 'simpan':
			case 'simpan_baru':Alert.show(wsErrmessage, l('Informasi'));break;
			case 'cekid' :
				o.vCurr = '';
//				if (vfplus=='')
				o.txtkode.setFocus();
				FlexGlobals.topLevelApplication.DefaultTanggalforDB
				if(wsErrmessage.indexOf("can't be empty") >= 0)
				{
					Alert.show(wsErrmessage, l('Informasi'));
				}
				else
				{
					Alert.show(l('Data sudah ada'), l('Informasi'));
				}
				break;
		}
	}
}

 
public function F_MasterSimpan(baru:Boolean, paket:String, data:String, sort:String = '', filter:String= ''):void
{
	switch(baru){
		case true://simpanbaru
			F_wsSimpan("simpan_baru", paket, o.vUserId, o.isUpdate, data, '',1,20,filter,sort);
			o.f_kondisi_awal();
			o.isUpdate = false;
			break;
		case false: //simpan
			F_wsSimpan("simpan", paket, o.vUserId, o.isUpdate, data, '', 1, 20,filter,sort);
			break;
	}
}

public function F_persiapanFormMaster():void
{
	var vSort:Boolean = true, vPosisi:int = 0, vCMI:ContextMenuItem, dg:DataGrid, mb:MenuBar, arr:Array = new Array;
	o = m[idmod].child;
	dg = o.dg; mb = o.mb;
	o.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_KEY_DOWN);
	// Config Paging
	o.txtpaging.addEventListener(flash.events.KeyboardEvent.KEY_DOWN, f_pagingKEY_DOWN);
	o.txtpaging.restrict = "0-9";
	o.txtpaging.setStyle("textAlign","center");
	o.txtpaging.text = 0;
	o.txtpaging.maxChars = 4;
	o.imgfirst.addEventListener(MouseEvent.CLICK, f_firstCLICK);
	o.imgprevious.addEventListener(MouseEvent.CLICK, f_previousCLICK);
	o.imgnext.addEventListener(MouseEvent.CLICK, f_nextCLICK);
	o.imglast.addEventListener(MouseEvent.CLICK, f_lastCLICK);
	
	function f_KEY_DOWN(e:KeyboardEvent):void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		switch(e.keyCode){
			case Keyboard.HOME:
				if(int(o.txtpaging.text)>1)
					f_formMasterTampil();
				break;
			case Keyboard.PAGE_UP:
				if(int(o.txtpaging.text)-1>0)
					f_formMasterTampil(int(o.txtpaging.text)-1);
				break;
			case Keyboard.PAGE_DOWN:
				f_formMasterTampil(int(o.txtpaging.text)+1);
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
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(int(o.txtpaging.text)>1)
			f_formMasterTampil();
	}
	
	function f_previousCLICK():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(int(o.txtpaging.text)-1>0)
			f_formMasterTampil(int(o.txtpaging.text)-1);
	}
	
	function f_nextCLICK():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		f_formMasterTampil(int(o.txtpaging.text)+1);
	}
	
	function f_lastCLICK():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		f_formMasterTampil(int(-1));
	}
	
	function f_pagingKEY_DOWN(e:KeyboardEvent):void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(e.keyCode == Keyboard.ENTER)
			if(int(o.txtpaging.text)>0)
				f_formMasterTampil(int(o.txtpaging.text));
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
	vCMI = new ContextMenuItem(l('Edit'))     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_edit);  
	dg.contextMenu.customItems.push(vCMI);    
	vCMI = new ContextMenuItem(l('Hapus Data'));     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_hapus);   
	dg.contextMenu.customItems.push(vCMI);    
	vCMI = new ContextMenuItem(l('History'))     
	vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_history);  
	dg.contextMenu.customItems.push(vCMI); 
	function f_GRID_ROLL_OVER(e:GridEvent):void
	{ 
		vgMyRow = e.rowIndex;
		vgMyCol = e.columnIndex;
	}
	
	function  f_GRID_CHANGE(e:GridSelectionEvent):void{
		f_refreshIDmodule(dg);
		o = m[module].child;
		o.f_grid_change();
	}
	
	function f_dgMENU_SELECT():void
	{
		dg.setSelectedCell(vgMyRow, vgMyCol);
	}
	
	function f_dgKEY_DOWN(e:KeyboardEvent):void
	{
		if(e.ctrlKey && e.keyCode == Keyboard.C)
		{
			m.getClipboard = vpVarCurrent;
			flash.system.System.setClipboard(vpVarCurrent);
		}
		else if(e.keyCode == Keyboard.DELETE)
		{
			if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 3) == true)
			Alert.show(l('Hapus data ini ?'), l('Informasi'), 3, null, function(e:Object):void{	
				if(e.detail == Alert.YES)
					f_hapus()
			});
		}
	}
	
	// Config Menu Bar
	var arrMenuBar:ArrayCollection = new ArrayCollection([
		{label:"Tampilkan", icon:"tbpreview"},
		{label:"Reset", icon:"tbreset"},
		{label:"Tambah", icon:"tbadd"},
		{label:"Edit", icon:"tbedit"},
		{label:"Hapus", icon:"tbdelete"},
		{label:"Cetak", icon:"tbprint"},
		{label:"Export", icon:"tbexport", children:[
			{label:"Ms.Excel", icon:"tbexcel", data:"1"},
			{label:"Html", icon:"tbhtml", data:"2"},
			{label:"Ms.Word", icon:"tbword", data:"3"},
			{label:"Text", icon:"tbtxt", data:"4"},
			{label:"Image", icon:"tbtif", data:"5"},
			{label:"Excel Data", icon:"tbexcel", data:"6"}]
		},
		{label:"Catatan", icon:"tbnote"},
		{label:"Files", icon:"tbattachment"}]);
	F_settingmb(mb, arrMenuBar);
	mb.menuBarItems[0].addEventListener(MouseEvent.CLICK, f_tampilkan); // tampil
	mb.menuBarItems[1].addEventListener(MouseEvent.CLICK, f_reset); // reset
	mb.menuBarItems[2].addEventListener(MouseEvent.CLICK, f_tambah); // tambah
	mb.menuBarItems[3].addEventListener(MouseEvent.CLICK, f_edit); // edit
	mb.menuBarItems[4].addEventListener(MouseEvent.CLICK, f_hapus); // hapus
	mb.menuBarItems[5].addEventListener(MouseEvent.CLICK, f_cetak); // cetak
	mb.menuBarItems[7].addEventListener(MouseEvent.CLICK, f_catatan); // bantuan
	mb.menuBarItems[8].addEventListener(MouseEvent.CLICK, f_files); // bantuan
	mb.addEventListener(MenuEvent.ITEM_CLICK,function  klik(e:MenuEvent):void{
		f_refreshIDmodule(dg);
		o = m[module].child;
		o.vfileextention=int(e.item.data);
		o.f_cetak();
	})
	function f_dgSort_changing(e:GridSortEvent):void
	{
		var ob:Object = m[idmod].child.dg.columns.getItemAt(int(e.columnIndices)).dataField;
		if(ob != "no")
		if(vSort == true){ 
			vSort = false;
			f_formMasterTampil(1, String(ob));
		}else if(vPosisi != int(e.columnIndices)){
			vSort = false;
			f_formMasterTampil(1, String(ob));
		}else{
			vSort = true;dg.dataProvider.removeAll();
			f_formMasterTampil(1, ob+' DESC');
		}
	}
	
	function f_dgDOUBLE_CLICK():void
	{
		if(m[idmod].child.arr[8][0] == 1 && m[idmod].child.arr[8][1] != 11){
			f_edit();
		}else if(m[idmod].child.arr[8][0] == 11){
			f_edit();
		}
	}
	
	function f_tampilkan():void
	{
		f_refreshIDmodule()
		f_formMasterTampil();
	}
	
	function f_hapus():void
	{
		if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 3) == true){
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(dg.dataProviderLength > 0)
			Alert.show(l('Data akan dihapus, lanjutkan?'), l('Konfirmasi'), 3, null, function(e:Object):void{	
				if(e.detail == Alert.YES)
					F_wsDelete(sptParam+'terkaitFormMaster', o.arr[6], userid, o.f_paramDelete(), 1, 20, o.f_filterdata(), o.arr[1]);
			});
		}
	}
	
	function f_reset():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		o.f_kondisiAwal();
	}
	
	function f_tambah():void
	{
		if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 1) == true){
			f_refreshIDmodule(dg);
			o = m[module].child;
			openFormX(l(o.arr[2]), 1, o.arr[3], o.arr[5], o.arr[4])
		}
	}
	
	function f_edit():void
	{
		if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 2) == true){
			f_refreshIDmodule(dg);
			o = m[module].child;
			o.saveas = false;
			if(dg.dataProviderLength > 0)
				openFormX(l(o.arr[2]), 1, o.arr[3], o.arr[5], o.arr[4], 't1', '@')
		}
	}
	
	function f_cetak():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		o.f_cetak();
	}
	
	function  f_catatan():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(dg.selectedCell != null)
			F_OpenNotesMaster(o.arr[9][0], o.arr[9][1], o.arr[9][2], o.arr[9][3]);
	}
	
	function f_files():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		if(dg.selectedCell != null){
			F_OpenFilesMaster(o.arr[9][0], o.arr[9][1], o.arr[9][2], o.arr[9][3]);
		}
	}
	
	function f_bantuan():void
	{
		f_refreshIDmodule(dg);
		o = m[module].child;
		F_help(o.arr[5][0], o.arr[5][1]);
	}
	
	function f_history():void
	{
		if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 12) == true){
			var arr4:Object = {};
			f_refreshIDmodule(dg);
			o = m[module].child;
			if(o.arrHistory.length == 4)arr4 = o.arrHistory[3];
			else arr4 = "";
			openFormX(m.ArrTab[MySuperTab.selectedIndex].label+" History", 0, "mod/m0/m0_HistoryMasterData.swf",0,0,'t1',"x",[dg, o.arrHistory[0], o.arrHistory[1], o.arrHistory[2], arr4]);
		}
	}
	
}


protected var vMasterSort:String;
public function f_formMasterTampil(page:int = 1, sort:String = ''):void
{
	if(apaBisaAkses(m[idmod].child.arr[8][0], m[idmod].child.arr[8][1], 4) == true){
		f_refreshIDmodule()
		if(sort == '')sort = m[idmod].child.arr[1];
		vMasterSort = sort;
		F_wsSearch("★tampilForm", m[idmod].child.arr[0], page, 20, m[idmod].child.f_filterdata(), sort);
	}
}

public function F_dgRefresh(grid:DataGrid):void
{
	if(grid.dataProvider != null)
		this.callLater((grid.dataProvider as ArrayCollection).refresh);
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
	o = m[idmod].child
	f = o.a[3];
	s = o.cmbstatus.textInput.text;

	if(s.length == 0){
		
	}else if(s == 'Semua'){
		o.cmbstatus.selectedIndex = 0;
//		if(o.a[4] == true)
//			filter = TambahFilter(filter, "(case "+f+"inputuser WHEN "+userid+" THEN ("+f+"status = 0 OR "+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 5 OR "+f+"status = 6 OR "+f+"status = 7 OR "+f+"status = 8 OR "+f+"status = 9 OR "+f+"status = 10 OR "+f+"status = 11) else ("+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 5 OR "+f+"status = 6 OR "+f+"status = 7 OR "+f+"status = 8 OR "+f+"status = 9 OR "+f+"status = 10 OR "+f+"status = 11) end)");
//		else
//			filter = TambahFilter(filter, "(case "+f+"inputuser WHEN "+userid+" THEN ("+f+"status = 0 OR "+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 7) else ("+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 7) end)");
	}else if(s == 'Aktif'){
		o.cmbstatus.selectedIndex = 1;
		filter = TambahFilter(filter, ""+f+"status = 2 OR "+f+"status = 3 OR "+f+"status = 4 OR "+f+"status = 7");
		//	else if(s == 'Draft')
		//		filter = TambahFilter(filter, ""+f+"status = 0 AND "+f+"inputuser = "+userid);
	}else
		filter = TambahFilter(filter, ""+f+"status = "+o.cmbstatus.selectedItem.v);
	
	if(o.txtnotransaksi1.text != '' && o.txtnotransaksi2.text != '')
		filter = TambahFilter(filter, f+'notransaksi BETWEEN "'+o.txtnotransaksi1.text+'" AND "'+o.txtnotransaksi2.text+'"');
	
	if(o.dttgl1.text != '' && o.dttgl2.text != '')
		filter = TambahFilter(filter, f+'tgl BETWEEN "'+F_tglFormatDB(o.dttgl1.text)+'" AND "'+F_tglFormatDB(o.dttgl2.text)+'"'); 
	else if(o.dttgl1.text != '') 
		filter = TambahFilter(filter, f+'tgl >= "'+F_tglFormatDB(o.dttgl1.text)+'"'); 
	else if(o.dttgl2.text != '')
		filter = TambahFilter(filter, f+'tgl <= "'+F_tglFormatDB(o.dttgl2.text)+'"'); 
	
	if(o.txtlokasi.text != '')
		filter = TambahFilter(filter, f+'lokasi LIKE "%'+o.txtlokasi.text+'%"');
	
	if(o.txturaian.text != '')
		filter = TambahFilter(filter, f+'uraian LIKE "%'+o.txturaian.text+'%"');
	
	if(o.txtcatatan.text != '')
		filter = TambahFilter(filter, f+'catatan LIKE "%'+o.txtcatatan.text+'%"');
	
	if(o.txtcabang.text != '')
		filter = TambahFilter(filter, f+'cabang LIKE "%'+o.txtcabang.text+'%"');
	
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
		o = winC;
	else{
		f_refreshIDmodule();
		o = m[idmod].child;
	}
	o.f_setProperty(components, dataField, data, info);
}

public function set v_checkBoxHeader(data:Boolean):void
{
	m.chkBoxHeader = data;
	m.SetChkBoxHeader = true;
}

public function F_getProperty(components:String, dataField:String, data:Object, info:String = ''):Object
{
	if(m.bukacompencarian)
		o = winC;
	else
	{
		f_refreshIDmodule();
		o = m[idmod].child;
	}
	
	switch(components){
		case 'checkBoxHeader':
			if(m.SetChkBoxHeader)
			{
				m.SetChkBoxHeader = false;
				return m.chkBoxHeader;
			}
			break;
		case 'checkBox':  
			o.f_getProperty(dataField, data, info);
			break;
		case 'comboBox':  
			f_refreshIDmodule();
			if(o.hasOwnProperty('f_getProperty'))
			{
				return o.f_getProperty(components, dataField, data, info);
			}
			break;
		case 'custom':  
		case 'dinamis':  
			f_refreshIDmodule();
			if(m[idmod].hasOwnProperty('child'))
			{
				if(o != null)
				{
					if(o.hasOwnProperty('f_getProperty'))
					{
						return o.f_getProperty(components, dataField, data, info);
					}
				}
			}
			break;
	}
	return null;
}

public function F_tglSelisih(date1:String, date2:String):int
{
	return (hitunghari(date1)-hitunghari(date2));
	
	
	function hitunghari(date:String):int
	{
		var d1:Array = date.split('/'), i:int;
		var data:int = d1[0], bln:int = d1[1], thn:int = d1[2];
		if(bln > 1)
			for(i=1;i<bln;i++)
			{
				data += F_getDayCount(thn, i);
			}
		return data;
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
	var d:Date = new Date(year, month, 0);
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

public function F_tglPensiun(date: String, value: Number):String
{
	var d1:Array = date.split('/')
	var tanggal : Date = new Date(d1[2], d1[1]-1, d1[0]);
	tanggal.setMonth(tanggal.month + Number(value));
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
 
public function F_paramDate(date1:String, date2:String, pemisah:String = " s.d "):String
{
	if(date1.length > 0 && date2.length > 0)
		return date1+pemisah+date2;
	else if(date1.length > 0 && date2.length == 0)
		return date1+pemisah+"";
	else if(date1.length == 0 && date2.length > 0)
		return ""+pemisah+date2;
	else return ""+pemisah+"";
//	if(date1.length > 0 && date2.length > 0)
//		return date1+pemisah+date2;
//	else if(date1.length > 0 && date2.length == 0)
//		return date1+pemisah+DefaultTanggal;
//	else if(date1.length == 0 && date2.length > 0)
//		return m.DateFirst+pemisah+date2;
//	else return m.DateFirst+pemisah+DefaultTanggal;
}

public function F_AutoNoTransaksi(idtext:Object, Checkbox:Object):void{
	if(int(getSetting(0,'accounting','AutoNoTransaction'))==0){
		Checkbox.selected = false;
		Checkbox.enabled = false;
		idtext.text = ''; 
		idtext.enabled = true;
	}
	else if(int(getSetting(0,'accounting','AutoNoTransaction'))==1){
		Checkbox.selected = true;
		Checkbox.enabled = false;
		idtext.text = 'Auto'; 
		idtext.enabled = false;
	}else{
		Checkbox.selected = false;
		idtext.text = ''; 
		idtext.enabled = true;
	}
}

public function F_standartTransaksiTerkait(dg:DataGrid):void
{
	with(dg){
		editable = false;
		sortableColumns = false;
  		resizableColumns = true;
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

public function msgAlert(focus: Object, pesan: String):void
{ 
	f_refreshIDmodule();
	o = m[idmod].child;
	if(o.validasi == true)focus.setFocus();
	o.pesanAlert += l(pesan)+', ';
	if(focus == 'pesanAlert'){
		o.pesanAlert = o.pesanAlert.substr(0, o.pesanAlert.length-4);
		o.pesanAlert += " "+l("harus diisi dengan benar");
		Alert.show(l(o.pesanAlert), l('Informasi'));
	}
	o.validasi = false; 
}

public function f_mbLinks(moduleid:int, sumber:String):void
{
	var data:String;
	f_refreshIDmodule();
	o = m[idmod].child;
	if(o.hasOwnProperty('u'))
		data = o.u[0][sumber.toLocaleLowerCase()+'id'];
	else
		data = o.o[sumber.toLocaleLowerCase()+'id'];
	if(data > '0'){
		F_wsGetDataById(sptParam+'tampilFDTransaksiTerkait', 'M'+moduleid+'_'+sumber+'Terkait', data);
	}
}

public function f_bukaTransaksi(sumber:String, filter:String=''):void
{
	var mod:String, menu:String;
	mod = getNomor(sumber.toUpperCase(), 2);
	f_refreshIDmodule();
	o = m[idmod].child;
	o.o.bt = new ComOpenTransaction;
	PopUpManager.addPopUp(m[idmod].child.o.bt, this, true);
	PopUpManager.centerPopUp(m[idmod].child.o.bt);
	o.o.bt.setFocus();
	o.o.bt.sumber = sumber;
	o.o.bt.mod = mod;
	F_wsSearch(sptParam+'bukaTransaksi', 'M'+mod+'_'+sumber+'Search', 1, 10, filter, sumber.toLowerCase()+'inputtgl desc');
}

public function f_bukaTransaksiPOS(sumber:String, filter:String=''):void
{
	var mod:String, menu:String;
	mod = getNomor(sumber.toUpperCase(), 2);
	f_refreshIDmodule();
	o = m[idmod].child;
	o.o.bt = new ComOpenTransactionPOS;
	PopUpManager.addPopUp(m[idmod].child.o.bt, this, true);
	PopUpManager.centerPopUp(m[idmod].child.o.bt);
	o.o.bt.setFocus();
	o.o.bt.sumber = sumber;
	o.o.bt.mod = mod;
	F_wsSearch(sptParam+'bukaTransaksi', 'M'+mod+'_'+sumber+'Search', 1, 10, filter, sumber.toLowerCase()+'inputtgl desc');
}

public function F_settingmb(mb:MenuBar, arrMenuBar:ArrayCollection):void
{
	var mb:MenuBar, j:int, y:int;
	for(j=0;j<arrMenuBar.length;j++)
	{
		arrMenuBar[j].label = l(arrMenuBar[j].label);
		if(arrMenuBar[j].hasOwnProperty('children'))
		{
			for(y=0;y<arrMenuBar[j].children.length;y++)
			{
				arrMenuBar[j].children[y].label = l(arrMenuBar[j].children[y].label);
			}
		}
	}				
	mb.dataProvider = arrMenuBar;
	mb.labelField = "label";
	mb.iconField = "icon";
	mb.setStyle("cornerRadius", 0);
	mb.validateNow(); 
	for(j=0;j<mb.menuBarItems.length;j++)
	{
		mb.menuBarItems[j].toolTip = arrMenuBar[j].label;
	}
}

public var hargatotal:Number=0;
public function F_hargaTotal(grid:DataGrid, fieldharga:String = 'harga', fieldnilaisatuan:String = 'nilaisatuan', fieldsubtotal:String = 'subtotal', fieldjml:String = 'jml', fielddiskon:String = 'diskon', fieldjmldiskon:String = 'jmldiskon'):void
{
	var dgChild:Object;
	dgChild = grid.dataProvider.getItemAt(grid.selectedCell.rowIndex);
	hargatotal = dgChild[fieldharga] / dgChild[fieldnilaisatuan];
	dgChild[fieldharga] = hargatotal * dgChild[fieldnilaisatuan];
	dgChild[fieldjmldiskon] = F_diskonBertingkat(dgChild[fieldjml], dgChild[fieldharga], dgChild[fielddiskon]);
	dgChild[fieldsubtotal] = (dgChild[fieldjml] * dgChild[fieldharga]) - dgChild[fieldjmldiskon];
	F_dgRefresh(grid);
}

public function f_removeAlert():void
{
	PopUpManager.removePopUp(vAlert);
}

// -------------------------------Akhir fungsi-fungsi -----------------------------------------