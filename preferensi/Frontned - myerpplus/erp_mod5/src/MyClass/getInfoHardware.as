package MyClass
{
	import flash.events.Event;
	import flash.events.IOErrorEvent;
	import flash.events.ProgressEvent;
	import flash.events.SecurityErrorEvent;
	import flash.events.TimerEvent;
	import flash.net.Socket;
	import flash.system.Security;
	import flash.utils.Timer;
	
	import mx.collections.ArrayCollection;
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	import spark.components.DataGrid;

	public class getInfoHardware 
	{
		private var socketCetak:Socket, host_:String = FlexGlobals.topLevelApplication.m.POS_computerIP, port_:int = FlexGlobals.topLevelApplication.m.POS_port, dataKirim:String = "";
		private var arrQueue:ArrayCollection = new ArrayCollection, antrianSiap:Boolean = false, mauCetak:Boolean = false;
		private var TryConnectAgain:Boolean = true, idmod:String;
		
		public function getInfoHardware(IPHardware:String, PortHardware:int)
		{ 
			host_ = IPHardware;
			port_ = PortHardware;
			mauCetak = true;
			socketCetak = new Socket;
			idmod = FlexGlobals.topLevelApplication.idmod;
			socketCetak.addEventListener(Event.CONNECT, function():void
			{
				TryConnectAgain = true;
				socketCetak.writeUTFBytes("getInfo$");
				socketCetak.flush(); 
			});
			
			socketCetak.addEventListener(Event.CLOSE, function():void
			{
				TryConnectAgain = true;
				socketCetak.close();
			});
			
			socketCetak.addEventListener(SecurityErrorEvent.SECURITY_ERROR, function(e:SecurityErrorEvent):void
			{
				TryConnectAgain = true;
				antrianSiap = true;
				if(e.toString().indexOf("Error #2048") >= 0)
				{
					if(mauCetak)
					{
						mauCetak = false; 
						FlexGlobals.topLevelApplication.m[idmod].child.f_setProperty("getInfo", "getInfo", "notConnect");
					}
				}
				else
				{
					Alert.show("SECURITY_ERROR : "+e.toString(), "Informasi");
				}
			});
			
			socketCetak.addEventListener(IOErrorEvent.IO_ERROR, function (e:IOErrorEvent):void
			{
				TryConnectAgain = true;
				antrianSiap = true;
				if("Error #2031: Socket Error. URL: "+host_ == e.text)
				{
					if(mauCetak)
					{
						mauCetak = false; 
						FlexGlobals.topLevelApplication.m[idmod].child.f_setProperty("getInfo", "getInfo", "notConnect");
					}
				}
				else
				{
					Alert.show("IO_ERROR : "+e.text, "Informasi");
				}
			});
			
			socketCetak.addEventListener(ProgressEvent.SOCKET_DATA, socket_data);
			
			connect();
		}
		
		private function socket_data(e:Event):void
		{ 
			var data:String, ar:Array;
			while (socketCetak.bytesAvailable) 
			{ 
				data = socketCetak.readUTFBytes(socketCetak.bytesAvailable);
				FlexGlobals.topLevelApplication.m[idmod].child.f_setProperty("getInfo", "getInfo", data);
			}
		}
		
		public function connect():Boolean
		{
			if(socketCetak.connected)
			{
				return socketCetak.connected;
			}

			if(TryConnectAgain)
			{
				TryConnectAgain = false;
				Security.loadPolicyFile("xmlsocket://"+host_+":"+port_);
				socketCetak.connect(host_, port_);
			}
			return false;
		}
	}
}