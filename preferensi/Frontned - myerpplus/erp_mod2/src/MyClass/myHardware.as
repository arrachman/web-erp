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
	
	public class myHardware 
	{
		private var socketCetak:Socket, host_:String = FlexGlobals.topLevelApplication.m.POS_computerIP, port_:int = FlexGlobals.topLevelApplication.m.POS_port, dataKirim:String = "";
		private var no_kirim:int = 0, user:String, arrQueue:ArrayCollection = new ArrayCollection, antrianSiap:Boolean = false, tmr:Timer = new Timer(2000, 300), mauCetak:Boolean = false;
		private var TryConnectAgain:Boolean = true;
		
		public function myHardware()
		{ 
			user = FlexGlobals.topLevelApplication.ukode;
			
			socketCetak = new Socket;
			socketCetak.addEventListener(Event.CONNECT, function():void
			{
				tmr.stop();
				TryConnectAgain = true;
				no_kirim = 0;
				socketCetak.writeUTFBytes(user+"$");
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
				//				Alert.show("SECURITY_ERROR : "+e.toString(), "Informasi"); 
			});
			
			socketCetak.addEventListener(IOErrorEvent.IO_ERROR, function (e:IOErrorEvent):void
			{
				TryConnectAgain = true;
				antrianSiap = true;
				if("Error #2031: Socket Error. URL: "+host_ == e.text)
				{
					if(mauCetak)
					{
						tmr.reset();
						tmr.start();
						mauCetak = false; 
						Alert.show("Cetak pending !\nMyHardware aktifkan dulu", "Informasi");
					}
				}
				else
				{
					Alert.show("IO_ERROR : "+e.text, "Informasi");
				}
			});
			
			socketCetak.addEventListener(ProgressEvent.SOCKET_DATA, socket_data);
			
			tmr.start();
			
			tmr.addEventListener(TimerEvent.TIMER, function():void
			{
				connect();
			});
		}
		
		public function cetak(pelanggan:String = "", konten:DataGrid = null, bayar:Number = 0, diskon:Number = 0, notransaksi:String = "", kredit:Number = 0, debit:Number = 0, totaltransaksi:Number = 0):void
		{
			
			var i:int, row:String, field:String, detail:String, param:String, alamat1:String = "", alamat2:String = "";
			var namaPerusahaan:String, struk:int, user:String, namaPrint:String = "", jarak:int = 0, arr:Array;
			
			//namaPrint, struk, jarak, namaToko, user, pelanggan, detail, bayar, diskon, alamat1, alamat2, notransaksi
			if(konten.dataProvider == null)
			{
				return;
			}
			
			if(pelanggan.length == 0 || konten.dataProviderLength == 0 || bayar == 0)
			{
				return;
			}
			
			if(konten.dataProvider[i].kodebarang.length == 0)
			{
				return;
			}
			
			if(notransaksi.length == 0)
			{
				return;
			}
			
			row = "~";
			field = "|";
			param = "@p2@";
			namaPrint = FlexGlobals.topLevelApplication.m.POS_print;
			struk = FlexGlobals.topLevelApplication.m.POS_struk;
			jarak = FlexGlobals.topLevelApplication.m.POS_jarak;
			alamat1 = FlexGlobals.topLevelApplication.m.POS_alamat1;
			alamat2 = FlexGlobals.topLevelApplication.m.POS_alamat2;
			namaPerusahaan = FlexGlobals.topLevelApplication.m.NamaPerusahaan;
			user = FlexGlobals.topLevelApplication.unama;
			detail = "";
			
			for(i=0;i<konten.dataProviderLength;i++)
			{
				detail += konten.dataProvider[i].kodebarang+ field;
				detail += konten.dataProvider[i].namabarang + field;
				detail += konten.dataProvider[i].jml + field;
				detail += konten.dataProvider[i].harga + field;
				detail += konten.dataProvider[i].jmldiskon + field;
				detail += konten.dataProvider[i].subtotal;
				if(i < konten.dataProviderLength-1)
				{
					detail += row;
				}
			}
			
			arr = [namaPrint, struk, jarak, namaPerusahaan, user, pelanggan, detail, bayar, diskon, alamat1, alamat2, notransaksi, kredit, debit, totaltransaksi]
			dataKirim = "";
			for(i=0;i<arr.length;i++)
			{
				if(i > 0)
					dataKirim += param;
				dataKirim += arr[i];
			} 
			dataKirim = "cetak@p1@"+dataKirim+"@p1@"+(++no_kirim)
			arrQueue.addItem({data:dataKirim});
			
			mauCetak = true;
			
			if(connect() && antrianSiap)
			{
				antrianSiap = false;
				socketCetak.writeUTFBytes(arrQueue[0].data+"$");
				socketCetak.flush();
			} 
		}
		
		private function socket_data(e:Event):void
		{ 
			var data:String, ar:Array;
			while (socketCetak.bytesAvailable) 
			{ 
				data = socketCetak.readUTFBytes(socketCetak.bytesAvailable);
				if(data == "get_stream")
				{
					if(arrQueue.length > 0)
						arrQueue.removeItemAt(0);
					
					if(connect() && arrQueue.length > 0)
					{
						socketCetak.writeUTFBytes(arrQueue[0].data+"$");
						socketCetak.flush();
					}
					else
					{
						antrianSiap = true;
					}
				}
				else if(data == "sign_in")
				{
					if(connect() && arrQueue.length > 0)
					{
						socketCetak.writeUTFBytes(arrQueue[0].data+"$");
						socketCetak.flush();
					}
					else
					{
						antrianSiap = true;
					}
				}
			}
		}
		
		public function poleDisplay(data:String):void
		{
			
			var i:int, row:String, field:String, detail:String, param:String, alamat1:String = "", alamat2:String = "";
			
			dataKirim = data;
			dataKirim = "poleDisplay@p1@"+dataKirim+"@p1@"+(++no_kirim)
			arrQueue.addItem({data:dataKirim});
			
			mauCetak = true;
			
			if(connect() && antrianSiap)
			{
				antrianSiap = false;
				socketCetak.writeUTFBytes(arrQueue[0].data+"$");
				socketCetak.flush();
			} 
		}
		
		public function cashDrawer(data:String):void
		{
			
			var i:int, row:String, field:String, detail:String, param:String, alamat1:String = "", alamat2:String = "";
			
			dataKirim = "rhs";
			dataKirim = "cashDrawer@p1@"+dataKirim+"@p1@"+(++no_kirim)
			arrQueue.addItem({data:dataKirim});
			
			mauCetak = true;
			
			if(connect() && antrianSiap)
			{
				antrianSiap = false;
				socketCetak.writeUTFBytes(arrQueue[0].data+"$");
				socketCetak.flush();
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