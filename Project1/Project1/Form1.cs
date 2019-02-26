using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Project1
{    
    public partial class Form1 : Form
    {
        public string[,] libraryArray = new string[23, 2];
        public string[,] functionsArray = new string[28, 2];
        
        static int addressCounter=0;
        public Form1(){
            InitializeComponent();
            fillLibraryArray();
            functionsTable();// table için gereken array i doldurur..

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e){

            openFileDialog1.Title = "Please select the assembly file";
            openFileDialog1.Filter = "(*.asm)|*.asm|(*.basm)|*.basm";
            openFileDialog1.ShowDialog();
            txtFilename.Text = openFileDialog1.FileName;
            if (txtFilename.Text == "") return;

            //Read file into ListBox
            lstInstructions.Items.Clear();
            using (StreamReader sr = new StreamReader(txtFilename.Text)){
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null){
                    lstInstructions.Items.Add(line);
                }
            }
            lstInstructions.SelectedIndex = 0;
        }
        private int Integer(string p)
        {
            throw new NotImplementedException();
        }        
        private void Form1_Load(object sender, EventArgs e){

        }
        private string convertToDec1(string number)
        {// binary sayıyı - li de olsa decimal e çevirir.. yani 1010 => -6..
            string binary = number; string _decimal = ""; 
            if (binary[0] == '1')
            {
                binary = binary.Substring(1, binary.Length - 1);
                _decimal = (Convert.ToInt32(binary, 2) - 8).ToString(); 
            }
            else
            {
                _decimal = (Convert.ToInt32(binary, 2)).ToString(); 
            } 
            return _decimal;
        }         
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }        
        int Cycle = 0;// işlenen komut sayısını tutar..
        string SC = "0"; // T ' yi tutar.. // Bu kaldırılacak..
        Boolean flagMicroListBox=false;
        int indexFetchDecode = 0;
        int indexOperation = 0;
        Boolean flagDirectIndirectFinish = false;   
        Boolean flagNewInstruction = true;
        int PC = 0;
        Boolean flagHex = false;

        private void button2_Click(object sender, EventArgs e){
            
            String str1 = lstInstructions.SelectedItem.ToString();
                String str3 = takeStringWithoutComment(str1);
                str3 = str3.Replace("\t", " ");
                str3.TrimStart(' ');
                
            String[] array = str3.Trim(' ').Split(' ');            
            if(array[0]!="ORG" && str3 != ""){
                if(flagHex==true){
                    doEverythingBinary("Binary");
                }

                textBox10.Text = SC;
                if (lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1)
                {
                    textBox18.Text = Cycle.ToString();

                    String [] array1 = str1.Split(',');
                    if (flagDirectIndirectFinish)
                    {
                        textBox15.Clear(); textBox17.Clear();
                        textBox15.Text = array[0];
                        if (array1.Length == 1 && flagNewInstruction)
                        {
                            writeOperationsToMicroList(findOperationFromTable(array[0]));
                            flagNewInstruction = false;
                        }
                        if (listBox1.Items.Count > listBox1.SelectedIndex + 1)
                        {
                            Cycle++;
                            listBox1.SelectedIndex += 1;
                            SC = takeNegatie("DEC", (Convert.ToInt32(convertBinaryToDecimal(textBox10.Text)) + 1).ToString());
                            
                        }
                        else
                        {
                            flagMicroListBox = false;
                        }
                        if (array.Length == 3 && array[2] == "I")
                        {
                            textBox17.Text = "Indirect";
                        }
                        else if (array.Length == 2 || array.Length == 1)
                        {
                            textBox17.Text = "Direct";
                        }
                        doOperations(array[0], indexOperation);
                        indexOperation++;
                    }
                    if (!flagDirectIndirectFinish&&array1.Length == 1 && array.Length==3 && array[2] == "I")//direct indirect anlama
                    {
                        writeFetch_Decode(indexFetchDecode,true);
                        indexFetchDecode++;
                        Cycle++;
                        int a = Convert.ToInt32(convertBinaryToDecimal(textBox10.Text));
                        SC = takeNegatie("DEC", (a + 1).ToString());
                        if (indexFetchDecode > 3) { flagDirectIndirectFinish = true; }
                    }
                    else if (!flagDirectIndirectFinish)
                    {
                        writeFetch_Decode(indexFetchDecode,false);
                        indexFetchDecode++;
                        Cycle++;
                        SC = takeNegatie("DEC", (Convert.ToInt32(convertBinaryToDecimal(textBox10.Text)) + 1).ToString());
                        if (indexFetchDecode > 2) { flagDirectIndirectFinish = true; }
                    }
                    
                }
                //textBox10.Text = takeNegatie("DEC",(convertBinaryToDecimal(textBox10.Text)+1).ToString());    
                if (flagHex == true)
                {
                    doEverythingBinary("Hex");
                }
            }
            if ((array[0] == "ORG" || (!flagMicroListBox && lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1)))
            {
                lstInstructions.SelectedIndex += 1;
                if (str3 != "")
                {
                    flagMicroListBox = true;
                    flagNewInstruction = true;
                    listBox1.Items.Clear();
                    indexFetchDecode = 0; indexOperation = 0;
                    flagDirectIndirectFinish = false;
                    textBox17.Clear(); textBox15.Clear();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {           
            // --------------------------------------------------------------//
            addressCounter = 0;
            String segmentName = "";
            int amount = 0;// 0 ları koymak için tuttuğumuz bir counter
            int i1 = 0; int i2 = 0; int i3 = 0;// segmentlerin 0 kontrolü
            while (lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1)
            {// Segmentleri yazdırma..
                String str2 = lstInstructions.SelectedItem.ToString();
                String str1 = takeStringWithoutComment(str2);
                str1 = str1.Replace("\t", " ");
                str1 = str1.Trim(' ');
                if (str1 != "")
                {
                    String[] arrayOrg = str1.Split(' ');
                    Boolean flagJustOne = false;
                    if (arrayOrg.Length == 1)
                    {
                        flagJustOne = true;
                    }

                    int temptoKeepItemCount = 0;
                    temptoKeepItemCount = lstInstructions.SelectedIndex;
                    if (arrayOrg[0] == "ORG")
                    {
                        addressCounter = Convert.ToInt32(arrayOrg[2]);
                        segmentName = arrayOrg[1];
                    }
                    else
                    {
                        switch (segmentName)
                        {
                            case "C":
                                if (i1 == 0)
                                {
                                    writeZeroToSegmentCode(addressCounter);
                                    amount = addressCounter;
                                }
                                if (flagJustOne)
                                {
                                    listView5.Items.Add(takeNegatie("DEC", addressCounter.ToString()));
                                    listView5.Items[amount].SubItems.Add(findFromLibrary(arrayOrg[0]));
                                }
                                else
                                {
                                    listView5.Items.Add(Convert.ToString(convertToBinary(addressCounter)));
                                    if (findFromLibrary(arrayOrg[0].Trim(',')) == "" && str1.Split(',').Length != 2)
                                    {
                                        listView5.Items[amount].SubItems.Add(takeNegatie(arrayOrg[1], arrayOrg[2]));
                                    }
                                    else if (str1.Split(',').Length == 2)
                                    {
                                        string[] array1 = str1.Split(',');
                                        string[] array2 = array1[1].Trim(' ').Split(' '); 
                                        array1[0].Trim(' ');array2[0].Trim(' ');
                                        listView5.Items[amount].SubItems.Add(insertZero(findFromLibrary(array2[0])) + takeNegatie("DEC", findAddressOfLabel(array2[1])));
                                    }
                                    else
                                    {
                                        listView5.Items[amount].SubItems.Add(insertZero(findFromLibrary(arrayOrg[0])) + takeNegatie("DEC", findAddressOfLabel(arrayOrg[1])));
                                    }
                                }
                                i1++;
                                break;
                            case "D":
                                if (i2 == 0)
                                {
                                    writeZeroToSegmentData(addressCounter);
                                    amount = addressCounter;
                                }
                                if (flagJustOne)
                                {
                                    listView1.Items.Add(takeNegatie("DEC", addressCounter.ToString()));
                                    listView1.Items[amount].SubItems.Add(findFromLibrary(arrayOrg[0]));
                                }
                                else
                                {
                                    listView1.Items.Add(Convert.ToString(convertToBinary(addressCounter)));
                                    if (findFromLibrary(arrayOrg[0].Trim(',')) == "")
                                    {
                                        if (arrayOrg[1] == "HEX")
                                        {
                                            string a = convertToBinary(convertHexToDecimal(arrayOrg[2]));
                                            listView1.Items[amount].SubItems.Add(a);
                                        }
                                        else
                                        {
                                            listView1.Items[amount].SubItems.Add(takeNegatie(arrayOrg[1], arrayOrg[2]));
                                        }
                                    }
                                    else
                                    {
                                        listView1.Items[amount].SubItems.Add(findFromLibrary(arrayOrg[0]) + takeNegatie("DEC", findAddressOfLabel(arrayOrg[1])));
                                    }
                                }
                                i2++;
                                break;
                            case "S":
                                if (i3 == 0)
                                {
                                    writeZeroToSegmentStack(addressCounter);
                                    amount = addressCounter;
                                }
                                if (flagJustOne)
                                {
                                    listView4.Items.Add(takeNegatie("DEC", addressCounter.ToString()));
                                    listView4.Items[amount].SubItems.Add(findFromLibrary(arrayOrg[0]));
                                }
                                else
                                {
                                    listView4.Items.Add(Convert.ToString(convertToBinary(addressCounter)));
                                    if (findFromLibrary(arrayOrg[0].Trim(',')) == "")
                                    {
                                        listView4.Items[amount].SubItems.Add(takeNegatie(arrayOrg[1], arrayOrg[2]));
                                    }
                                    else
                                    {
                                        listView4.Items[amount].SubItems.Add(findFromLibrary(arrayOrg[0]) + takeNegatie("DEC", findAddressOfLabel(arrayOrg[1])));
                                    }
                                }
                                i3++;
                                break;
                        }
                        amount++;
                        rowCounter++;
                        addressCounter++;
                    }
                    lstInstructions.SelectedIndex = temptoKeepItemCount;

                }
                if (lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1)
                {
                    lstInstructions.SelectedIndex += 1;
                }
            }
            lstInstructions.SelectedIndex = 0;
        }
        public void writeOperationsToMicroList(String str) { 
            String [] array = str.Split(';');
            for (int i = 0; i < array.Length; i++)
            {
                listBox1.Items.Add(array[i]);
            }            
        }
        public string findOperationFromTable(string strToSearch) {
            string strReturn = "";
            for (int i = 0; i < functionsArray.GetLength(0); i++ )
            {
                if(strToSearch==functionsArray[i,0]){
                    strReturn = functionsArray[i, 1];
                }
            }
            return strReturn;
        }
        public string convertDecimalToBinaryORHex(string ChoseNumber, string str)// decimal sayı geliyor
        {
            string returnValue="";

            if (ChoseNumber == "Binary" && str != "")// decimal i binarye
            {
                returnValue=convertToBinary(Convert.ToInt16(str)).ToString();
            }
            else if (ChoseNumber == "Hex" && str != "")// decimal i hex e
            {
                returnValue = Convert.ToString(Convert.ToInt32(str), 16).ToString().ToUpper();
            }
            return returnValue;
        }

        //Get complement of AC
        public void getComplementOfAC(string s) //Erdem
        {
            if (convertBinaryToDecimal(textBox3.Text) == 0)
            {
                convertBinaryToDecimal("1"); //Get decimal value of 1
            }
            else if (convertBinaryToDecimal(textBox3.Text) == 1)
            {
                convertBinaryToDecimal("0"); //Get decimal value of 0
            }
        }
        //Transfer AC or PC to M[AR] 
        public string searchFromACorPC(string binaryNumberToSearch) //Erdem
        {
            string strReturn = "";
            if (textBox3.Text == binaryNumberToSearch)
            {
                strReturn = textBox3.Text;
            }
            return strReturn;
        }

        public void doOperations(String operationName, int index) {            
            switch(operationName){            
                case "OR":
                    if(index==0)
                        textBox5.Text = searchFromDataMemory(textBox7.Text);//DR←M[AR]
                    else if(index==1){
                        textBox3.Text = takeNegatie("DEC", (Convert.ToByte(convertBinaryToDecimal(textBox3.Text)) | Convert.ToByte(Convert.ToInt32(convertBinaryToDecimal(textBox5.Text)))).ToString());
                        textBox10.Text = "0";SC="0";
                    }//SC ←0             
                    break;
                case "AND":
                    if (index == 0)
                    textBox5.Text=searchFromDataMemory(textBox7.Text);//DR←M[AR]
                    else if (index == 1){
                        textBox3.Text = takeNegatie("DEC", (Convert.ToByte(convertBinaryToDecimal(textBox3.Text)) & Convert.ToByte(Convert.ToInt32(convertBinaryToDecimal(textBox5.Text)))).ToString());
                        textBox10.Text = "0"; SC = "0";//SC ←0
                    }
                    break;
                case "XOR":
                    if (index == 0)
                    textBox5.Text=searchFromDataMemory(textBox7.Text);//DR←M[AR]
                    else if (index == 1)
                    {
                        textBox3.Text = takeNegatie("DEC", (Convert.ToByte(convertBinaryToDecimal(textBox3.Text)) ^ Convert.ToByte(Convert.ToInt32(convertBinaryToDecimal(textBox5.Text)))).ToString());
                        textBox10.Text = "0"; SC = "0";//SC ←0
                    }
                    break;
                case "ADD":
                    if(index==0)
                        textBox5.Text=searchFromDataMemory(textBox7.Text);//DR←M[AR]
                    else if(index==1){
                        string a = (Convert.ToInt16(convertToDec1(textBox3.Text)) + Convert.ToInt16(convertToDec1(textBox5.Text))).ToString();
                        textBox3.Text = convertToBinary(Convert.ToInt16(a));// AC ← AC +DR                    
                        textBox10.Text = "0"; SC = "0";//SC ←0
                    }
                    break;
                case "LDA":
                    if (index == 0)
                        textBox5.Text = searchFromDataMemory(textBox7.Text);//DR‹M[AR]
                    else if (index == 1)
                    {
                        textBox3.Text = textBox5.Text;// AC ‹ DR                    
                        textBox10.Text = "0"; SC = "0";//SC ‹0
                    }
                    break;
                case "STA":
                    writeToDataMemory(textBox7.Text, textBox3.Text);  // Md[AR] ‹  AC 
                    textBox10.Text = "0"; SC = "0"; // SC  ‹  0
                    break;
                case "BUN":
                    textBox8.Text = textBox7.Text;  // PC ‹ AC
                    textBox10.Text = "0"; SC = "0";//SC ‹0
                    break;
                case "BSA":
                    if (index == 0)
                    {
                        writeToCodeMemory(textBox7.Text, textBox8.Text);// Mc[AR]  ‹   PC  
                        textBox7.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox7.Text) + 1).ToString());  // AR  ‹   AR + 1 

                    }
                    else if (index == 1)
                    {
                        textBox8.Text = textBox7.Text;  //PC  ‹  AR
                        textBox10.Text = "0"; SC = "0";   //  SC ‹  0
                    }
                    break;
                case "ISZ":
                    if (index == 0)
                    {

                        textBox5.Text = searchFromDataMemory(textBox7.Text); // DR  ‹  Md[AR]
                    }

                    else if (index == 1)
                    {
                        textBox5.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox5.Text) + 1).ToString());  // DR  ‹   DR + 1 
                    }
                    else if (index == 2)  //  Md[AR]  ‹  DR ,  if ( DR = 0 ) then  PC  <-  PC + 1  ,SC  <-  0 
                    {

                        writeToDataMemory(textBox7.Text, textBox3.Text); //  Md[AR]  ‹  DR 
                        if (textBox3.Text == "0")
                        {
                            textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString());  // PC  ‹   PC + 1 
                        }
                        textBox3.Text = "0";
                    }
                    break;
                case "CLA":
                    textBox3.Text = "0"; //AC ‹ 0

                    break;
                case "SZA":
                    if (textBox3.Text == "0") // if ( AC = 0 ) then  PC  ‹  PC + 1
                    {
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString());
                        textBox10.Text = "0"; SC = "0";//  PC  ‹  PC + 1
                    }
                    break;
                case "SNA":

                    if (textBox3.Text.Substring(0, 3) == "1") //if ( AC(3) = 1 ) then  PC  ‹   PC + 1
                    {
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString()); // PC  ‹   PC + 1
                        textBox10.Text = "0"; SC = "0";
                    }
                    break;
                case "CMA":
                    //AC  <-  AC’
                    int comp = 0;
                    if (textBox3.Text.Length == 1)
                        comp = 1;
                    else if (textBox3.Text.Length == 2)
                        comp = 3;
                    else if (textBox3.Text.Length == 3)
                        comp = 7;
                    else if (textBox3.Text.Length == 4)
                        comp = 15;
                    textBox3.Text = takeNegatie("DEC", (comp - convertBinaryToDecimal(textBox3.Text)).ToString());
                    break;
                case "INC":
                    textBox3.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox3.Text) + 1).ToString()); // AC ‹ AC + 1
                    textBox10.Text = "0"; SC = "0";
                    break;
                case "ASHR":
                    //E  <-  AC(3)  ,  AC  <-  shr  AC  ,  AC(3)<-  E 
                    textBox14.Text = textBox3.Text.Substring(0, 1); // E  <-  AC(3)
                    textBox3.Text = "0" + textBox3.Text.Substring(0, textBox3.Text.Length - 1);
                    textBox3.Text = textBox14.Text + textBox3.Text.Substring(1); //  AC(3)<-  E
                    textBox10.Text = "0"; SC = "0";
                    break;
                case "ASHL":
                    if (index == 0)
                    {
                        textBox14.Text = "0"; //  	E  <-  0
                        textBox3.Text = "0" + textBox3.Text.Substring(1, textBox3.Text.Length) + textBox3.Text.Substring(0, 1); //    AC  <-  shl AC 
                        textBox3.Text = textBox3.Text.Substring(0, textBox3.Text.Length - 1) + textBox14.Text;  //AC(0)<-  E 
                        textBox10.Text = "0"; SC = "0";
                    }
                    else if (index == 1)
                    {
                        textBox11.Text = "0";   // S  <-  0
                        textBox10.Text = "0"; SC = "0";
                    }

                    break;
                case "SZE":
                    if (textBox14.Text == "0")
                    {
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString()); //if ( E = 0 ) then  PC  ‹  PC + 1
                    }
                    break;
                case "HLT":
                    textBox10.Text="0";
                    MessageBox.Show(
                        "<< Assembly Code Completed >>"
                        );
                    break;
                //case "INP":
                //    if (comboBox2.SelectedItem != null)
                //    {
                //        textBox3.Text = convertToBinary(Convert.ToInt16(comboBox2.SelectedItem)).ToString();
                //    }
                //    else
                //    {
                //        textBox3.Text = "0000";
                //    }
                //    button5.Text = "0";
                //    break;
                case "INP":

                    if (index == 0)
                    {

                        textBox3.Text = textBox1.Text; //AC(0-3)  ‹  INPR
                    }
                    else if (index == 1)
                    {
                        //AC(0-3)  <-  INPR
                        textBox3.Text = textBox1.Text;
                    }
                    break;
                case "Push":
                    if (index == 0)
                    {//Ms[SP]  <-  DR
                        writeToStackMemory(textBox11.Text, textBox5.Text);
                    }
                    else if (index == 1)
                    {
                        //SP  <-  SP  +  1  ,  SC  <-  0
                        textBox11.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox11.Text) + 1).ToString());
                        textBox10.Text = "0"; SC = "0";

                    }

                    break;
                case "Pop":
                    if (index == 0)
                    {
                        // SP  <-  SP   -  1
                        textBox11.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox11.Text) - 1).ToString());
                    }
                    else if (index == 1)
                    {
                        //DR  <-  Ms[SP]  ,  SC  <-  0
                        textBox5.Text = searchFromStackMemory(textBox11.Text);
                        textBox10.Text = "0"; SC ="0";

                    }

                    break;
                case "SZEmpty":
                    // if ( SP = 0 )  then  PC <-  PC + 1  ,  SC  <-  0
                    if (textBox11.Text == "0")
                    {
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString());
                        textBox10.Text = "0"; SC = "0";
                    }

                    break;
                case "SZFull":
                    //qB4T3:	if ( SP = 7 )  then  PC  <-  PC + 1  ,  SC  <-  0
                    if (textBox11.Text == "7")
                    {
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString());
                        textBox10.Text = "0"; SC = "0";
                    }
                    break;
            }
        }        
        public string takeNegatie(string ChoseNumber,string str) {

            if (ChoseNumber == "DEC" && str!="")// decimal i binarye
            {                
               return Convert.ToString(convertToBinary(Convert.ToInt32(str)));
            }
            else if (ChoseNumber == "HEX" && str != "")// decimal i hex e
            {
                string a =Convert.ToString(Convert.ToInt16(convertHexToDecimal(str).ToString()), 16);
                return a;
            }
            else
            {
                return "";
            }
        }
        public string findFromLibrary(string valueToSearch) {// kütüphaneden value'sunu alabilmek için..
            string value = "";
            for (int i = 0; i < libraryArray.GetLength(0); i++){
                if (libraryArray[i,0]==valueToSearch){
                    value = libraryArray[i,1];
                    break;
                }
            }
            return value;
        }
        public string findAddressOfLabel(string labelToSearch) {// LDA D gibi ifadenin value'sunu bulmak.. 54 gibi..
            string address = "";
            lstInstructions.SelectedIndex = 0;
            int addresscounter1 = 0;
            while (lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1){
                String str2 = lstInstructions.SelectedItem.ToString();
                String str1 = takeStringWithoutComment(str2);
                str1 = str1.Replace("\t", " ");
                str1 =str1.Trim(' ');
                if (str1 != "")
                {
                    String[] arrayOrg = str1.Split(' ');
                    if (arrayOrg[0] == "ORG")
                    {
                        addresscounter1 = Convert.ToInt32(arrayOrg[2]);
                    }
                    else
                    {
                        String[] array = str1.Split(',');

                        if (array.Length == 2)
                        {// labellileri yazdırma
                            String[] arraybosluk1 = str1.Split(' ');

                            string x = arraybosluk1[0].Trim(',');
                            if (x == labelToSearch)
                            {
                                address = addresscounter1.ToString();
                            }
                        }
                        addresscounter1++;
                    }
                }
                lstInstructions.SelectedIndex += 1;
            }
            if (address == ""){
                address = labelToSearch;
            }
            return address;
        }             
        public string takeStringWithoutComment(String str2) {
            String str1 = "";
            int k = 0;
            while (str2.Length > k && str2.Substring(k, 1) != "/")
            {
                str1 = str1 + str2.Substring(k, 1);
                k++;
            }
            return str1;
        }
        public void writeFetch_Decode(int index, bool flagIndirect)
        {
            textBox15.Clear();
            
            if (index < 3)
            {                
                string[] array = functionsArray[0, 1].Split(';');
                textBox15.Text = array[index];
                textBox17.Text = "Fetch-Decode";

                switch (index)
                {
                    case 0:
                        textBox7.Text = textBox8.Text;//AR ← PC
                        break;
                    case 1:
                        if (!flagIndirect)
                        {
                            textBox6.Text =   searchFromCodeMemory(textBox7.Text);//IR ← M[AR]
                            if (flagHex)
                            {
                                textBox6.Text = "0" + chooseBinaryOrHex("Binary", textBox6.Text);
                            }
                        }
                        else
                        {
                            textBox6.Text =  searchFromCodeMemory(textBox7.Text);//IR ← M[AR]
                            if (flagHex)
                            {
                                textBox6.Text = "1" + chooseBinaryOrHex("Binary", textBox6.Text);
                            }
                        }
                        textBox8.Text = takeNegatie("DEC", (convertBinaryToDecimal(textBox8.Text) + 1).ToString()); //PC ← PC+1 // decimale çevrilip toplanmalı.. Sonra tekrar binary e..
                        break;
                    case 2:
                        // textBox7.Text = textBox8.Text;//D0,...,D15 ← Decode IR(4-7),, ??
                        if (textBox6.Text.Length<4)
                        {
                            textBox6.Text=takeNegatie("DEC",convertHexToDecimal(textBox6.Text).ToString()).ToString();
                        }
                        textBox7.Text = textBox6.Text.Substring(textBox6.Text.Length - 4, 4);//AR←IR(0-3)
                        textBox12.Text = textBox6.Text.Substring(0, 1);// I←IR(8)
                        break;
                }
            }
            else
            {
                textBox15.Text = functionsArray[1, 1];
                textBox7.Text = searchFromDataMemory(textBox7.Text); // AR←M[AR] //indirect// datamemoryden okudukk..
            }
        }       
        private string convertToBinary(int number) // gelen - li decimal sayıyı signed bnary sayıya dönüştürür..
        {
            string temp = "";
            string binary = Convert.ToString(number, 2).PadLeft(4, '0');
            if (number < 0)
            {
                for (int i = binary.Length - 1; i >= binary.Length - 4; i--)
                {
                    temp = temp + binary[i];
                }
                string temp2 = "";
                for (int i = temp.Length - 1; i >= 0; i--)
                {
                    temp2 = temp2 + temp[i];
                }
                temp = temp2;
            }
            else
            {
                temp = binary;
            }
            return temp;
        }        
        public void doEverythingBinary(string numberType){
            foreach (ListViewItem item in listView5.Items)// code memory çevirme
            {
                //item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
                if (item.SubItems[0].Text.Length == 2)
                {
                    item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text.Substring(0,1)) + chooseBinaryOrHex(numberType, item.SubItems[0].Text.Substring(1,1));
                }
                else
                {
                    item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                }
            }
            foreach (ListViewItem item in listView1.Items)// data memory çevirme // değişecek içi.. hex 57. Bunu hm binary hem de hex olark çevirmek gerekiyor 
            {
                item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
                //string value = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
                //item.SubItems[1].Text = chooseBinaryOrHex(numberType, value.Substring(value.Length - 4, 4))+chooseBinaryOrHex(numberType, value.Substring(value.Length - 4, 4));

            }
            foreach (ListViewItem item in listView4.Items)// Stack memory çevirme
            {
                item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
            }

            textBox1.Text = chooseBinaryOrHex(numberType, textBox1.Text);
            textBox2.Text = chooseBinaryOrHex(numberType, textBox2.Text);
            textBox3.Text = chooseBinaryOrHex(numberType, textBox3.Text);
            textBox4.Text = chooseBinaryOrHex(numberType, textBox4.Text);
            textBox5.Text = chooseBinaryOrHex(numberType, textBox5.Text);
            textBox6.Text = chooseBinaryOrHex(numberType, textBox6.Text);
            textBox7.Text = chooseBinaryOrHex(numberType, textBox7.Text);
            textBox8.Text = chooseBinaryOrHex(numberType, textBox8.Text);
            //textBox9.Text = chooseBinaryOrHex(numberType, textBox9.Text);
            textBox10.Text = chooseBinaryOrHex(numberType, textBox10.Text);
            textBox11.Text = chooseBinaryOrHex(numberType, textBox11.Text);
            textBox12.Text = chooseBinaryOrHex(numberType, textBox12.Text);
            textBox13.Text = chooseBinaryOrHex(numberType, textBox13.Text);
            textBox14.Text = chooseBinaryOrHex(numberType, textBox14.Text);
        }
        public string chooseBinaryOrHex(string type, string number) {
            string returnvalue = "";
            if(type=="Binary"){// Hex den binary e
                if (number.Length == 2)
                {
                    //item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text.Substring(0, 1)) + chooseBinaryOrHex(numberType, item.SubItems[0].Text.Substring(1, 1));
                    returnvalue = insertZero(convertDecimalToBinaryORHex("Binary", (convertHexToDecimal(number.Substring(0, 1))).ToString())) + insertZero(convertDecimalToBinaryORHex("Binary", (convertHexToDecimal(number.Substring(1, 1))).ToString()));
                }
                else
                {
                    returnvalue = insertZero(convertDecimalToBinaryORHex("Binary", (convertHexToDecimal(number)).ToString()));
                }                
            }else{// Binary geliyor. Bunu Hexa ya çeviricez..
                returnvalue = convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(number)).ToString());   
            }
            return returnvalue;
        }
        String showHolder = "Binary";
        public void ShowBinaryOrHex(string numberType) {
            foreach (ListViewItem item in listView5.Items)// code memory çevirme
            {
                item.SubItems[0].Text = chooseBinaryOrHex(numberType,item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);     
            }
            foreach (ListViewItem item in listView1.Items)// data memory çevirme // değişecek içi.. hex 57. Bunu hm binary hem de hex olark çevirmek gerekiyor 
            {
                item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
                //string value = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
                //item.SubItems[1].Text = chooseBinaryOrHex(numberType, value.Substring(value.Length - 4, 4))+chooseBinaryOrHex(numberType, value.Substring(value.Length - 4, 4));

            }
            foreach (ListViewItem item in listView4.Items)// Stack memory çevirme
            {
                item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
            }
            foreach (ListViewItem item in listView3.Items)// Labelleri çevirme
            {
                //item.SubItems[0].Text = chooseBinaryOrHex(numberType, item.SubItems[0].Text);
                item.SubItems[1].Text = chooseBinaryOrHex(numberType, item.SubItems[1].Text);
            }
            textBox1.Text = chooseBinaryOrHex(numberType, textBox1.Text);
            textBox2.Text = chooseBinaryOrHex(numberType, textBox2.Text);
            textBox3.Text = chooseBinaryOrHex(numberType, textBox3.Text);
            textBox4.Text = chooseBinaryOrHex(numberType, textBox4.Text);
            textBox5.Text = chooseBinaryOrHex(numberType, textBox5.Text);
            textBox6.Text = chooseBinaryOrHex(numberType, textBox6.Text);
            textBox7.Text = chooseBinaryOrHex(numberType, textBox7.Text);
            textBox8.Text = chooseBinaryOrHex(numberType, textBox8.Text);
            //textBox9.Text = chooseBinaryOrHex(numberType, textBox9.Text);
            textBox10.Text = chooseBinaryOrHex(numberType, textBox10.Text);
            textBox11.Text = chooseBinaryOrHex(numberType, textBox11.Text);
            textBox12.Text = chooseBinaryOrHex(numberType, textBox12.Text);
            textBox13.Text = chooseBinaryOrHex(numberType, textBox13.Text);
            textBox14.Text = chooseBinaryOrHex(numberType, textBox14.Text);
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        public string searchFromCodeMemory(string binaryNumberToSearch) {
            string strReturn = "";
            foreach (ListViewItem item in listView5.Items) { 
                if (item.Text == binaryNumberToSearch) {
                    strReturn=item.SubItems[1].Text; // buda o değer 
                    break;
                } 
            }
            return strReturn;
        }
        public string searchFromDataMemory(string binaryNumberToSearch){
            string strReturn = "";
            foreach (ListViewItem item in listView1.Items) { 
                if (item.Text == binaryNumberToSearch) {
                    strReturn=item.SubItems[1].Text; // buda o değer 
                    break;
                } 
            }
            return strReturn;
        }
        public string searchFromStackMemory(string binaryNumberToSearch)
        {
            string strReturn = "";
            foreach (ListViewItem item in listView4.Items)
            {
                if (item.Text == binaryNumberToSearch)
                {
                    strReturn = item.SubItems[1].Text; // buda o değer 
                    break;
                }
            }
            return strReturn;
        }
        public void functionsTable(){// ; ların yeri boşlukla değiştirilecek..// Bunlar ekranda göstermek için.. İşlemleri yapmak için ayrıca bir fonksiyona yazılabilir.
            functionsArray[0, 0] = "Fecth-Decode"; functionsArray[0, 1] = "AR ← PC;IR ← M[AR] , PC ← PC+1;D0,...,D15 ← Decode IR(4-7),AR←IR(0-3), I←IR(8)";
            functionsArray[1, 0] = "Indirect"; functionsArray[1, 1] = "AR ← M[AR]";
            functionsArray[2, 0] = "OR"; functionsArray[2, 1] = "DR ← M[AR];AC ← AC V DR, SC ← 0";
            functionsArray[3, 0] = "AND"; functionsArray[3, 1] = "DR ← M[AR];AC ← AC n DR";
            functionsArray[4, 0] = "XOR"; functionsArray[4, 1] = "DR ← M[AR];AC ← AC XOR DR , SC ← 0";
            functionsArray[5, 0] = "ADD"; functionsArray[5, 1] = "DR ← M[AR];AC ← AC + DR ,E ← Cout, SC ← 0";
            functionsArray[6, 0] = "LDA"; functionsArray[6, 1] = "DR ← M[AR];AC ← DR,SC ← 0";
            functionsArray[7, 0] = "STA"; functionsArray[7, 1] = "M[AR] ← AC,SC ← 0";
            functionsArray[8, 0] = "BUN"; functionsArray[8, 1] = "PC ← AR,SC ← 0";
            functionsArray[9, 0] = "BSA"; functionsArray[9, 1] = "M[AR] ← PC, AR← AR + 1; PC ← AR,SC ← 0";
            functionsArray[10, 0] = "ISZ"; functionsArray[10, 1] = "DR ← M[AR];DR← DR + 1;M[AR] ← DR,if (DR==0) then (PC←PC+1), SC←0";            
            functionsArray[11, 0] = "CLA"; functionsArray[11, 1] = "AC ← 0";
            functionsArray[12, 0] = "SZA"; functionsArray[12, 1] = "if (AC=0) then (PC ← PC + 1)";
            functionsArray[13, 0] = "SNA"; functionsArray[13, 1] = "if(AC(3)=1) then (PC ← PC + 1)";
            functionsArray[14, 0] = "CMA"; functionsArray[14, 1] = "AC← AC değil";
            functionsArray[15, 0] = "INC"; functionsArray[15, 1] = "AC ← AC + 1";
            functionsArray[16, 0] = "ASHR"; functionsArray[16, 1] = "AC ← shr AC, AC(3) ← AC(2), E ← 0";
            functionsArray[17, 0] = "ASHL"; functionsArray[17, 1] = "AC ← shr AC, AC(0) ← 0, E ← 0";
            functionsArray[18, 0] = "SZE"; functionsArray[18, 1] = "if(E=0) then (PC ← PC + 1)";
            functionsArray[19, 0] = "HLT"; functionsArray[19, 1] = "S ← 0";                  
            functionsArray[20, 0] = "INP"; functionsArray[20, 1] = "AC(0-3) ← INPR, FGI ← 0";
            functionsArray[21, 0] = "SKI"; functionsArray[21, 1] = "if(FGI=1) then (PC←PC+1)";
            functionsArray[22, 0] = "ION"; functionsArray[22, 1] = "IEN ← 1";
            functionsArray[23, 0] = "IOF"; functionsArray[23, 1] = "IEN ← 0";    
        }
        public void fillLibraryArray()
        {
            libraryArray[0, 0] = "OR"; libraryArray[0, 1] = "0001";
            libraryArray[1, 0] = "AND"; libraryArray[1, 1] = "0010";
            libraryArray[2, 0] = "XOR"; libraryArray[2, 1] = "0011";
            libraryArray[3, 0] = "ADD"; libraryArray[3, 1] = "0100";
            libraryArray[4, 0] = "LDA"; libraryArray[4, 1] = "0101";
            libraryArray[5, 0] = "STA"; libraryArray[5, 1] = "0110";
            libraryArray[6, 0] = "BUN"; libraryArray[6, 1] = "1000";
            libraryArray[7, 0] = "BSA"; libraryArray[7, 1] = "1001";
            libraryArray[8, 0] = "ISZ"; libraryArray[8, 1] = "1111";
            libraryArray[9, 0] = "CLA"; libraryArray[9, 1] = "0001";
            libraryArray[10, 0] = "SZA"; libraryArray[10, 1] = "0010";
            libraryArray[11, 0] = "SNA"; libraryArray[11, 1] = "0011";
            libraryArray[12, 0] = "CMA"; libraryArray[12, 1] = "0100";
            libraryArray[13, 0] = "INC"; libraryArray[13, 1] = "0101";
            libraryArray[14, 0] = "ASHR"; libraryArray[14, 1] = "0111";
            libraryArray[15, 0] = "ASHL"; libraryArray[15, 1] = "1000";
            libraryArray[16, 0] = "SZE"; libraryArray[16, 1] = "0000";
            libraryArray[17, 0] = "HLT"; libraryArray[17, 1] = "1001";
            libraryArray[18, 0] = "INP"; libraryArray[18, 1] = "X111";
            libraryArray[19, 0] = "Push"; libraryArray[19, 1] = "X001";
            libraryArray[20, 0] = "Pop"; libraryArray[20, 1] = "X010";
            libraryArray[21, 0] = "SZEmpty"; libraryArray[21, 1] = "X011";
            libraryArray[22, 0] = "SZFull"; libraryArray[22, 1] = "X100";
        }
        public void writeZeroToSegmentData(int amount)
        {
            int rowCounter = 0;
            for (int i = 0; i < amount; i++)
            {
                listView1.Items.Add(Convert.ToString(convertToBinary(rowCounter)));
                listView1.Items[rowCounter].SubItems.Add(takeNegatie("DEC", "0"));
                rowCounter++;
            }
        }
        public void writeZeroToSegmentStack(int amount)
        {
            int rowCounter = 0;
            for (int i = 0; i < amount; i++)
            {
                listView4.Items.Add(Convert.ToString(convertToBinary(rowCounter)));
                listView4.Items[rowCounter].SubItems.Add(takeNegatie("DEC", "0"));
                rowCounter++;
            }
        }
        public void writeZeroToSegmentCode(int amount)
        {
            int rowCounter = 0;
            for (int i = 0; i < amount; i++)
            {
                listView5.Items.Add(Convert.ToString(convertToBinary(rowCounter)));
                listView5.Items[rowCounter].SubItems.Add(takeNegatie("DEC", "0"));
                rowCounter++;
            }
        }
        private void txtFilename_TextChanged(object sender, EventArgs e)
        {

        }
        public double convertBinaryToDecimal(String str)
        {
            char[] binaryNumber = new char[64];
            binaryNumber = str.ToCharArray();
            double decimalnumber = 0;
            int power = binaryNumber.Length - 1;
            for (int i = 0; i < binaryNumber.Length; i++)
            {
                if (binaryNumber[i] == '1')
                {
                    decimalnumber += Math.Pow(2, power);
                    power--;
                }
                else
                {
                    power--;
                }
            }
            return decimalnumber;
        }
        public void writeToDataMemory(string AR, string data)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Text == AR)
                {
                    item.SubItems[1].Text = data;
                }
            }
        }
        public void writeToCodeMemory(string AR, string PC)
        {
            foreach (ListViewItem item in listView5.Items)
            {
                if (item.Text == AR)
                {
                    item.SubItems[1].Text = PC;
                }
            }
        }
        public void writeToStackMemory(string SP, string DR)
        {
            foreach (ListViewItem item in listView5.Items)
            {
                if (item.Text == SP)
                {
                    item.SubItems[1].Text = DR;
                }
            }
        }
        public int convertHexToDecimal(String str)
        {            
            string Value = "";
            switch (str) {             
                case "a":
                    Value = "10";
                    break;
                case "b":
                    Value = "11";
                    break;
                case "c":
                    Value = "12";
                    break;
                case "d":
                    Value = "13";
                    break;
                case "e":
                    Value = "14";
                    break;
                case "f":
                    Value = "15";
                    break;
                default:
                    Value=str;
                    break;
                case "A":
                    Value = "10";
                    break;
                case "B":
                    Value = "11";
                    break;
                case "C":
                    Value = "12";
                    break;
                case "D":
                    Value = "13";
                    break;
                case "E":
                    Value = "14";
                    break;
                case "F":
                    Value = "15";
                    break;
            }
            if(str.Length!=1){
                string a="";
                for (int i = 0; i < str.Length; i++)
                {
                    a += insertZero(takeNegatie("DEC", str.Substring(i,1)));
                }
                Value =convertBinaryToDecimal(a).ToString();
            }
            return Convert.ToInt32(Value);
        }
        public string thusComplement(string str) {
            string temp = ""; 
            for (int i = 0; i < str.Length; i++) { 
                if (str[i] == '0') {
                    temp += '1'; 
                } else if (str[i] == '1') {
                    temp += '0'; 
                } 
            } 
            temp = convertBinaryToDecimal(temp).ToString(); 
            temp = convertToBinary(Convert.ToInt32(temp) + 1);
            return temp;
        }
        public string negativeNumber(String number) {
            string strToReturn = "";
            Boolean flagFirstOne = true;
            number = convertToBinary(Convert.ToInt32(number));
            for (int i = 0; i < 4; i++)
            {
                if (number.Substring(4 - i, 1) == "1" && flagFirstOne)
                {
                    flagFirstOne = false;
                    strToReturn = number.Substring(4 - i, 1) + strToReturn;
                }
                else if(!flagFirstOne){
                    strToReturn = number.Substring(4 - i, 1) + strToReturn;
                }
            }
            return strToReturn;
        }
        public string insertZero(string str) { 
            string value="";
            if (str.Length < 5)
            {
                if (str.Length == 1)
                {
                    string a = "000" + str;
                    value = a;
                }
                else if (str.Length == 2)
                {
                    string a = "0" + str;
                    value = a;
                }
                else if (str.Length == 3)
                {
                    string a = "0" + str;
                    value = a;
                }
                else
                {
                    value = str;
                }
            }else if(str.Length>4){
                if (str.Length == 5)
                {
                    string a = "000" + str;
                    value = a;
                }
                else if (str.Length == 6)
                {
                    string a = "0" + str;
                    value = a;
                }
                else if (str.Length == 7)
                {
                    string a = "0" + str;
                    value = a;
                }
                else
                {
                    value = str;
                }            
            }
            return value;
        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        int rowCounter = 0;
        private void button1_Click_1(object sender, EventArgs e)
        {
            string a = "-6";
            convertToBinary(Convert.ToInt32(a));
            while (lstInstructions.Items.Count > lstInstructions.SelectedIndex + 1)
            {// labelleri yazdırma

                String str2 = lstInstructions.SelectedItem.ToString();
                String str1 = takeStringWithoutComment(str2);
                str1 = str1.Replace("\t", " ");
                str1.TrimStart(' ');
                if (str1 != "")
                {
                    String[] arrayOrg = str1.Split(' ');
                    if (arrayOrg[0] == "ORG")
                    {
                        addressCounter = Convert.ToInt32(arrayOrg[2]);
                        if (arrayOrg[1] == "C")
                        {
                            PC = Convert.ToInt32(arrayOrg[2]);
                            textBox8.Text = takeNegatie("DEC", PC.ToString());
                        }
                    }
                    else
                    {
                        String[] array = str1.Split(',');

                        if (array.Length == 2)
                        {// labellileri yazdırma
                            String[] arraybosluk1 = str1.Split(' ');

                            string x = arraybosluk1[0].Trim(',');
                            listView3.Items.Add(x);
                            listView3.Items[rowCounter].SubItems.Add(takeNegatie("DEC", addressCounter.ToString()));
                            rowCounter++;
                        }
                        addressCounter++;
                    }
                }
                lstInstructions.SelectedIndex += 1;
            }
            lstInstructions.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString()!=showHolder)
            {
                if (comboBox1.SelectedItem.ToString() == "Hex")
                {
                    flagHex = true;
                }
                else {
                    flagHex = false;
                }
                showHolder = comboBox1.SelectedItem.ToString();
                ShowBinaryOrHex(comboBox1.SelectedItem.ToString());
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public string CheckSum(String str) {
            
            int sum=0;
            for (int i = 0; i < str.Length; i+=2)
            {
                string c=str.Substring(i,2);
                string d = chooseBinaryOrHex("Binary", c);
                
               int a = Convert.ToInt32(convertBinaryToDecimal(d));
               sum = sum + a;
            }
            return convertToBinary(sum);
        }

        private void button8_Click(object sender, EventArgs e)
        {           
            folderBrowserDialog1.ShowNewFolderButton = true;

            // Kontrolü göster
            String directory = "";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                directory = folderBrowserDialog1.SelectedPath;
            }

            StreamWriter codeMemoryFile = File.CreateText(directory + "\\" + "codeMemory" + ".hex");
            codeMemoryFile.Close();
            StreamWriter write1 = new StreamWriter(directory + "\\" + "codeMemory" + ".hex");
            
            foreach (ListViewItem item in listView5.Items)
            {
                String a ="";
                a=":02000"+chooseBinaryOrHex("Hex",item.SubItems[0].Text)+"0000";// 2 si sabit 2 si value için
                if (item.SubItems[1].Text.Length==8)
                {
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(thusComplement(CheckSum(a.Substring(1, a.Length - 1))))).ToString());
                }                    
                else if (item.SubItems[1].Text.Length == 4) {
                    a = a + "0";
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    string d = "";
                    string k = convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(CheckSum(a.Substring(1, a.Length - 1)))).ToString());
                    if (k.Length == 1)
                    {
                        d = convertDecimalToBinaryORHex("Hex",convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", ("0" + k)))).ToString());
                    }
                    else {
                        d = convertDecimalToBinaryORHex("Hex", convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", k))).ToString());
                    }
                    a = a+d;                 
                }
                write1.WriteLine(a);               
            }
            write1.WriteLine(":00000001FF"); 
            write1.Close();
            StreamWriter dataMemoryFile = File.CreateText(directory + "\\" + "dataMemory" + ".hex");
            dataMemoryFile.Close();
            StreamWriter write2 = new StreamWriter(directory + "\\" + "dataMemory" + ".hex");

            foreach (ListViewItem item in listView1.Items)
            {
                String a = "";
                a = ":02000" + chooseBinaryOrHex("Hex", item.SubItems[0].Text) + "0000";// 2 si sabit 2 si value için
                if (item.SubItems[1].Text.Length == 8)
                {
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    a += thusComplement(CheckSum(a.Substring(1, a.Length - 1)));
                }
                else if (item.SubItems[1].Text.Length == 4)
                {
                    a = a + "0";
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    string d = "";
                    string k = convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(CheckSum(a.Substring(1, a.Length - 1)))).ToString());
                    if (k.Length == 1)
                    {
                        d = convertDecimalToBinaryORHex("Hex", convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", ("0" + k)))).ToString());
                    }
                    else
                    {
                        d = convertDecimalToBinaryORHex("Hex", convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", k))).ToString());
                    }
                    a = a + d;            
                }
                write2.WriteLine(a);
            }
            write2.WriteLine(":00000001FF"); 
            write2.Close();

            StreamWriter stackMemoryFile = File.CreateText(directory + "\\" + "stackMemory" + ".hex");
            stackMemoryFile.Close();
            StreamWriter write3 = new StreamWriter(directory + "\\" + "stackMemory" + ".hex");

            foreach (ListViewItem item in listView4.Items)
            {
                String a = "";
                a = ":02000" + chooseBinaryOrHex("Hex", item.SubItems[0].Text) + "0000";// 2 si sabit 2 si value için
                if (item.SubItems[1].Text.Length == 8)
                {
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    a += thusComplement(CheckSum(a.Substring(1, a.Length - 1)));
                }
                else if (item.SubItems[1].Text.Length == 4)
                {
                    a = a + "0";
                    a += convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(item.SubItems[1].Text)).ToString());
                    string d = "";
                    string k = convertDecimalToBinaryORHex("Hex", (convertBinaryToDecimal(CheckSum(a.Substring(1, a.Length - 1)))).ToString());
                    if (k.Length == 1)
                    {
                        d = convertDecimalToBinaryORHex("Hex", convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", ("0" + k)))).ToString());
                    }
                    else
                    {
                        d = convertDecimalToBinaryORHex("Hex", convertBinaryToDecimal(thusComplement(chooseBinaryOrHex("Binary", k))).ToString());
                    }
                    a = a + d;        
                }
                write3.WriteLine(a);
            }
            write3.WriteLine(":00000001FF"); 
            write3.Close();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void listView5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
