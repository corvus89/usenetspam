package stemmer;

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.HashMap;
import java.util.Map;

public class Main {

    static Map<String, String> dict = new HashMap<String, String>();

    public static String stemming(String input){
        String output = dict.get(input);
        if(input != null) {
            return output;
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

    public static void main(String[] args) throws FileNotFoundException, IOException {
        load("/home/corvus/Pulpit/dictbezpl.txt");
        System.out.println(stemming("kowalczykach"));
        System.out.println(stemming("fajhajhfajfajksdfa"));
    }
}
