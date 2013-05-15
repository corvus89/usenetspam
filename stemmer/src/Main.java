

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.HashMap;
import java.util.Map;

public class Main {

    static Map<String, String> dict = new HashMap<String, String>();

    public static String stemming(String input){
        String output = dict.get(input);
        if(output != null) {
            return output+" ";
        } else {
            return "";
        }
    }

    public static void load(String pathToDict) throws FileNotFoundException, IOException {
        FileInputStream fstream = new FileInputStream(pathToDict);
        DataInputStream in = new DataInputStream(fstream);
        BufferedReader br = new BufferedReader(new InputStreamReader(in));
        String strLine;
        while ((strLine = br.readLine()) != null) {
            String[] temp = strLine.split(" ");
            dict.put(temp[0], temp[1]);
        }
    }
    
    public static void computeDirectory(String path) {
        
        File folder = new File(path);
        File[] listOfFiles = folder.listFiles();
        
        for (int i = 0; i < listOfFiles.length; i++) {
            
            if (listOfFiles[i].isFile()) {
                Stemm(listOfFiles[i], listOfFiles[i].getName());
              
            }
        }
    }
    
    public static void Stemm(File file, String outputName) {   
        System.out.println(outputName);
        try {
            InputStream inputStream = new FileInputStream(file);
            BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream, "windows-1250"));//new FileReader(path)
            
            String line;
            String newWords = "";
            
            while ((line = reader.readLine()) != null) {
            //if((line = reader.readLine()) != null) {
                line = line.trim();
                String[] words = line.split(" ");
                
                int wordsSize = words.length;
                //String[] newWords = new String[wordsSize];
                for(int i=0; i<wordsSize;i++){
                    newWords += stemming(words[i]);
                }
                newWords += "\n";
            }
            //if(newWords.trim().length()>150){ //255
                BufferedWriter out = new BufferedWriter(new FileWriter("stemmed_files/"+outputName));
                out.write(newWords.trim());
                out.close();
            //}
            
            reader.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }

    public static void main(String[] args) throws FileNotFoundException, IOException {
        load("dictbezpl.txt");
        //System.out.println(stemming("kowalczykach"));
        //System.out.println(stemming("fajhajhfajfajksdfa"));
        
        File dir = new File("stemmed_files");
        dir.mkdir();
        if (args[0].length()>0) {computeDirectory(args[0]);}
        else{computeDirectory("input");}
    }
    
    
}
