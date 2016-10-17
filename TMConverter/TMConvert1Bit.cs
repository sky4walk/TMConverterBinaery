// André Betz
// http://www.andrebetz.de
using System;
using System.Collections;
using TM2Train;

namespace TMConverter
{
	/// <summary>
	/// Summary description for TMConvert1Bit.
	/// </summary>
	public class TMConvert1Bit
	{
		private TMLoader m_TM = new TMLoader();
		private string m_TMName = null;
		private ArrayList m_Symbols = new ArrayList();
		private int m_BitLen = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="TMName">Dateiname der zu konvertierenden Turing Maschine Definition</param>
		public TMConvert1Bit(string TMName)
		{
			m_TMName = TMName;
		}
		/// <summary>
		/// konvertiert eine n-Bit Turing Maschine in eine 1-Bit Turng Maschine
		/// </summary>
		/// <returns></returns>
		public string Convert()
		{
			string strStart = null;
			string strTape = null;
			string strTrans = null;

			if(m_TM.Load(m_TMName))
			{
				int States = CountStates();
				m_BitLen = CalculateBitSize(m_Symbols.Count);
				strStart = "("+m_TM.StartState+"_,"+m_TM.StartTapePos*m_BitLen+")";
				strTape = ConvertTape();
				strTrans = ConvertTransitions();				
			}

			return FormatTM(m_TMName,strStart,strTape,strTrans);
		}
		/// <summary>
		/// formatiert die ausgabedatei
		/// </summary>
		/// <param name="Datnam">Dateiname</param>
		/// <param name="strStart">Start</param>
		/// <param name="strTape"></param>
		/// <param name="strTrans"></param>
		/// <returns></returns>
		private static string FormatTM(string Datnam,string strStart,string strTape,string strTrans)
		{
			string result = null;
			if(Datnam!=null&&strStart!=null&&strTape!=null&&strTrans!=null)
			{
				result = "# konvertierte Turing Maschine "+Datnam+"\r\n";
				result += "# http://www.AndreBetz.de \r\n\r\n";
				result += "# Initialisierung\r\n";
				result += strStart+"\r\n";
				result += "# Band\r\n";
				result += strTape+"\r\n";
				result += "# Transitionen\r\n";
				result += strTrans+"\r\n";
			}
			return result;
		}
		/// <summary>
		/// konvertiert die Transitionen
		/// </summary>
		/// <returns>konvertierte Transitionen</returns>
		private string ConvertTransitions()
		{
			ArrayList colStates = null;
			string Transitions = "";
			TMState sts = m_TM.GetStates;
			int NodesCnt = NodesTotal(m_BitLen);
			while(sts!=null)
			{
				sts = GetNextState(sts,ref colStates);
				if(colStates.Count>0)
				{
					string StateSymbl = ((TMState)colStates[0]).GetStateF()+"_";
					string Trans = "# Zustand: " + StateSymbl + "\r\n";
					for(int NodesNr=1;NodesNr<NodesCnt;NodesNr++)
					{
						string StateNrBinaer = GenerateBinNr(NodesNr);
						string StateNrBinaerBefore = GenerateBinNr(GetTreeNrBefore(NodesNr));
						int rdSymbl = ((NodesNr+1)%2);
						Trans += CreateTrans(StateSymbl+StateNrBinaerBefore,rdSymbl.ToString(),StateSymbl+StateNrBinaer,rdSymbl.ToString(),"R",(rdSymbl!=0));
					}
					Trans += "\r\n";
					for(int i=0;i<colStates.Count;i++)
					{
						TMState actSts = (TMState)colStates[i];
						string AddSymbl = ConvertSymbol(actSts.GetRead());;
						for(int Depth=0;Depth<m_BitLen;Depth++)
						{
							Trans += CreateTrans(StateSymbl+AddSymbl,"0",StateSymbl+AddSymbl+"Z","0","L",false);
							Trans += CreateTrans(StateSymbl+AddSymbl,"1",StateSymbl+AddSymbl+"Z","1","L",true);
							AddSymbl+="Z";
						}
						Trans += "\r\n";
						char[] writeSymbl = ConvertSymbol(actSts.GetWrite()).ToCharArray();
						if(actSts.GetMove().ToUpper().Equals("R"))
						{
							for(int Depth=0;Depth<writeSymbl.Length-1;Depth++)
							{
								Trans += CreateTrans(StateSymbl+AddSymbl,"0",StateSymbl+AddSymbl+"W",writeSymbl[Depth].ToString(),"R",false);
								Trans += CreateTrans(StateSymbl+AddSymbl,"1",StateSymbl+AddSymbl+"W",writeSymbl[Depth].ToString(),"R",true);
								AddSymbl+="W";
							}
							Trans += CreateTrans(StateSymbl+AddSymbl,"0",actSts.GetStateN()+"_",writeSymbl[writeSymbl.Length-1].ToString(),"R",false);
							Trans += CreateTrans(StateSymbl+AddSymbl,"1",actSts.GetStateN()+"_",writeSymbl[writeSymbl.Length-1].ToString(),"R",true);
						}
						else
						{
							for(int Depth=0;Depth<writeSymbl.Length;Depth++)
							{
								Trans += CreateTrans(StateSymbl+AddSymbl,"0",StateSymbl+AddSymbl+"W",writeSymbl[Depth].ToString(),"R",false);
								Trans += CreateTrans(StateSymbl+AddSymbl,"1",StateSymbl+AddSymbl+"W",writeSymbl[Depth].ToString(),"R",true);
								AddSymbl+="W";
							}
							Trans += "\r\n";
							for(int Depth=0;Depth<writeSymbl.Length*2-1;Depth++)
							{
								Trans += CreateTrans(StateSymbl+AddSymbl,"0",StateSymbl+AddSymbl+"Z","0","L",false);
								Trans += CreateTrans(StateSymbl+AddSymbl,"1",StateSymbl+AddSymbl+"Z","1","L",true);
								AddSymbl+="Z";
							}
							Trans += CreateTrans(StateSymbl+AddSymbl,"0",actSts.GetStateN()+"_","0","L",false);
							Trans += CreateTrans(StateSymbl+AddSymbl,"1",actSts.GetStateN()+"_","1","L",true);
						}
						Trans += "\r\n";
					}
					Transitions +=Trans;
				}
			}
			return Transitions;
		}
		/// <summary>
		/// schreibt eine Transition raus
		/// </summary>
		/// <param name="FSts">first state</param>
		/// <param name="rd">gelesenes zeichen</param>
		/// <param name="NSts">next state</param>
		/// <param name="wt">zu schreibendes zeichen</param>
		/// <param name="M">bewegungsrichtung</param>
		/// <returns>ausgabe der Transition</returns>
		private string CreateTrans(string FSts,string rd,string NSts, string wt,string M,bool NewLine)
		{
			string Trans = "";
			Trans += "(";
			Trans += FSts;
			Trans += ",";
			Trans += rd;
			Trans += ",";
			Trans += NSts;
			Trans += ",";
			Trans += wt;
			Trans += ",";
			Trans += M;
			Trans += ")";
			if(NewLine)
			{
				Trans += "\r\n";
			}
			else
			{
				Trans += "\t";
			}
			return Trans;
		}
		/// <summary>
		/// berechnet die Anzahl der Knoten der baumtiefe
		/// </summary>
		/// <param name="Depth">Baumtiefe</param>
		/// <returns>Anzahl der Knoten</returns>
		private int NodesTotal(int Depth)
		{
			int Fak = 0;
			for(int i=0;i<=Depth;i++)
			{
				Fak += CalculateStatesPerDepth(i);
			}
			return Fak;
		}
		/// <summary>
		/// berechnet die vorgehende BaumNr des Knotens
		/// </summary>
		/// <param name="NodeNr">aktuelle Baumnummer</param>
		/// <returns>vorhergehende Knotennummer</returns>
		private int GetTreeNrBefore(int NodeNr)
		{
			return (NodeNr-1)>>1;
		}
		/// <summary>
		/// Generiert aus der BaumknotenNummer den Binärstring
		/// </summary>
		/// <param name="NodeNr">BaumknotenNummer</param>
		/// <returns></returns>
		private string GenerateBinNr(int NodeNr)
		{
			string res = "";
			int cnt = 1;
			for(int Depth=1;Depth<=m_BitLen;Depth++)
			{
				int CntStates = CalculateStatesPerDepth(Depth);
				for(int StatNr=0;StatNr<CntStates;StatNr++)
				{
					if(cnt==NodeNr)
					{
						return FillBinaer(Dez2Bin(StatNr),Depth);
					}
					cnt++;
				}
			}
			return res;
		}
		/// <summary>
		/// Look for the next state
		/// </summary>
		/// <param name="FirstSts">first State</param>
		/// <param name="colStates">Liste der zusammengefassten Zustände</param>
		/// <returns>next state</returns>
		private TMState GetNextState(TMState FirstSts, ref ArrayList colStates)
		{
			colStates = new ArrayList();
			TMState resState = null;
			if(FirstSts!=null)
			{
				string StateSymbl = FirstSts.GetStateF();
				string NxtStateSymbl = StateSymbl;
				while(FirstSts!=null&&StateSymbl.Equals(NxtStateSymbl))
				{
					colStates.Add(FirstSts);
					FirstSts = FirstSts.GetNext();
					if(FirstSts!=null)
					{
						NxtStateSymbl = FirstSts.GetStateF();
					}
				}
				resState = FirstSts;
			}
			return resState;
		}
		/// <summary>
		/// Convertiert das Turing Band
		/// </summary>
		/// <returns>konvertierte Band</returns>
		private string ConvertTape()
		{
			string strBand = "(";
			Tape tp = m_TM.GetTape;
			while(tp!=null)
			{
				string symbl = tp.GetSign();
				string NewSymbl = ConvertSymbol(symbl);
				strBand += NewSymbl;
				tp = tp.GetNext();
			}
			return strBand+")";
		}
		private int CalculateStatesPerDepth(int Depth)
		{
			int rowStates = 1;
			for(int i=0;i<Depth;i++)
			{
				rowStates *= 2;
			}
			return rowStates;
		}
		/// <summary>
		/// Generate converted Symbol
		/// </summary>
		/// <param name="symbl">Symbol</param>
		/// <returns>Converted Symbol</returns>
		private string ConvertSymbol(string symbl)
		{
			string NewSymbl = "";
			int SymblPos = FindSymbolInList(symbl);
			if(SymblPos>=0)
			{
				NewSymbl = Dez2Bin(SymblPos);
				NewSymbl = FillBinaer(NewSymbl,m_BitLen);
			}
			return NewSymbl;
		}
		/// <summary>
		/// füllt die Binärzahl vorne mit 0en auf
		/// </summary>
		/// <param name="Binaer">Binärzahl</param>
		/// <param name="len">auf die länge auffüllen</param>
		/// <returns>neue Binärzahl mit führenden 0en</returns>
		private static string FillBinaer(string Binaer,int len)
		{
			string NewSymbl = Binaer;
			for(int i=Binaer.Length;i<len;i++)
			{
				NewSymbl = "0" + NewSymbl;
			}
			return NewSymbl;
		}
		/// <summary>
		/// konvertiert Dezimal nach Binär
		/// </summary>
		/// <param name="Dez">Dezimalzahl</param>
		/// <returns>Binärzahl als String</returns>
		private static string Dez2Bin(int Dez)
		{
			string binaer = "";
			if(Dez==0)
				binaer = Dez.ToString();
			while(Dez>0)
			{
				int res = Dez % 2;
				Dez = Dez >> 1;
				binaer = res.ToString() + binaer;
			}
			return binaer;
		}
		/// <summary>
		/// Find Symbol in Arraylist
		/// </summary>
		/// <param name="symbl">Symbol</param>
		/// <returns>position number in Array</returns>
		private int FindSymbolInList(string symbl)
		{
			for(int i=0;i<m_Symbols.Count;i++)
			{
				if(symbl.Equals((string)m_Symbols[i]))
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// errechnet die Bitgrösse
		/// </summary>
		/// <param name="SymbolCount">Anzahl der Symbole</param>
		/// <returns>Bitzahl n</returns>
		static private int CalculateBitSize(int SymbolCount)
		{
			int Count = 0;

			while(SymbolCount>0)
			{
				SymbolCount = SymbolCount >> 1;
				Count++;
			}

			return Count;
		}
		/// <summary>
		/// zählt die anzahl der Transitionen und füllt die Liste der Symbole auf
		/// </summary>
		/// <returns>Anzahl der Zustände</returns>
		private int CountStates()
		{
			int CountStates = 0;
			m_Symbols.Clear();

			TMState sts = m_TM.GetStates;
			while(sts!=null)
			{
				CountStates++;
				string symbl = sts.GetRead();
				AddSymbol(symbl);
				sts = sts.GetNext();
			}
			return CountStates;
		}
		/// <summary>
		/// nimmt ein neues Symbol in die Liste auf
		/// </summary>
		/// <param name="symbl">Symbol</param>
		/// <returns>true:hinzugenommen false:schon vorhanden</returns>
		private bool AddSymbol(string symbl)
		{
			for(int i=0;i<m_Symbols.Count;i++)
			{
				if(symbl.Equals((string)m_Symbols[i]))
				{
					return false;
				}
			}
			m_Symbols.Add(symbl);
			return true;
		}
	}
}
