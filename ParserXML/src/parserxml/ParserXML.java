import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.Timer;
import java.util.TimerTask;

/**
 *
 * @author Seth
 */
public class ParserXML {
    
    static int contentLimit = 100;
    static long index = 1;
    static int tag_last_index = 0;
    static ArrayList<String[]> indexer = new ArrayList<String[]>();
    static ArrayList<String> tagi = new ArrayList<String>();

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        
        File dir = new File("parsed_files");
        dir.mkdir();
        
        createTemplate();
        initIndex();
        
        tagi.add("content");
        if (args[0].equals("manual")) {
            for (int i = 1; i < args.length; i++) {
                createBody(args[i]);
                updateTemplate();
                createIndex();
            }
            closeTemplate();
        }else{
            System.out.println("How about NO?");
        }
        //computeDirectory(args[0]);
        //closeTemplate();
        //System.out.println(tag_last_index);
        //createTemplate2();
        //createIndex(); 
    }
    
    public static void computeDirectory(String path) {
        
        File folder = new File(path);
        File[] listOfFiles = folder.listFiles();
        
        for (int i = 0; i < listOfFiles.length; i++) {
            
            if (listOfFiles[i].isFile()) {
                createBody2(listOfFiles[i], listOfFiles[i].getName());
                updateTemplate();
                createIndex();
            }
        }
    }
    
    public static void createBody(String path) {
        String outputName = path.substring(path.lastIndexOf("/")).replace("/", "");
        System.out.println(outputName);
        
        //ArrayList<String> tagi = new ArrayList<String>();
        //tagi.add("contnet");
        long lineNumber = 1;
        String[] indexerRow = new String[4];
        String filename = outputName;
        int part = 0;
        boolean newrow_flag = true;
        boolean content_flag = false;
        boolean tag_flag = false;
        try {
            InputStream inputStream = new FileInputStream(path);
            BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream, "iso-8859-2"));//new FileReader(path)
            BufferedWriter out = new BufferedWriter(new FileWriter("parsed_files/"+outputName + ".xml"));
            String line;
            String tag = "";
            String cont = "";
            int counter = 0;
            
            while ((line = reader.readLine()) != null) {
                line = line.trim();
                if (newrow_flag && line.matches("Path:.*")) {
                    indexerRow[0] = Long.toString(index);
                    indexerRow[1] = Long.toString(lineNumber);
                    out.write("<sphinx:document id=\"" + index + "\">\n");
                    lineNumber++;
                    newrow_flag = false;
                    tag_flag = true;
                }
                if (content_flag) {
                    if (line.contains("-=-=-=-=-=-=-=-=============----------===")) {
                        if (counter < contentLimit) {
                            out.write("<content><![CDATA[[");
                            out.write(cont);
                            out.write("\n]]></content>\n");
                            lineNumber = lineNumber+ counter + 2;
                        }
                        out.write("</sphinx:document>\n");
                        
                        indexerRow[2] = Long.toString(lineNumber);
                        indexerRow[3] = filename;
                        lineNumber++;
                        indexer.add(indexerRow);
                        indexerRow = new String[4];
                        cont = "";
                        counter = 0;
                        index++;
                        if(lineNumber>150000){
                            part++;
                            lineNumber = 1;
                            out.close();
                            filename = outputName+ "_part" + part;
                            out = new BufferedWriter(new FileWriter("parsed_files/"+ filename + ".xml"));
                        }
                        
                        content_flag = false;
                        tag = "";
                        newrow_flag = true;
                        
                    } else if (counter < contentLimit) {
                        if (!line.startsWith(">") && line.length() > 1) {
                            line = replaceInLine(line);
                            cont = cont + "\n" + line;
                            counter++;
                        }
                    }
                    
                } else if(tag_flag){
                    line = replaceInLine(line);
                    
                   
                    
                    if (line.matches("^[A-Za-z]+[A-Za-z-]*: .*")) {


                        //if (line.indexOf(": ") > 0) {
                        if (!tag.equals("")) {
                            out.write("]]></" + tag + ">\n");
                        }
                        int separator = line.indexOf(": ");
                        tag = line.substring(0, line.indexOf(": ")).trim().toLowerCase();
                        if (!tagi.contains(tag)) {
                            tagi.add(tag);
                        }
                        out.write("<" + tag + "><![CDATA[["
                                + line.substring(separator + 1).trim());
                        lineNumber++;
                    } else if(line.matches("\\s.*")){
                        out.write(" "+line.trim());
                       
                    }
                    if (line.contains("Xref: ")) {
                        content_flag = true;
                        tag_flag = false;
                        out.write("]]></" + tag + ">\n");
			//tag = "";
                        //out.write("      <content>");
                    }
                }
            }
            //out.write("</sphinx:docset>\n");
            out.close();
            reader.close();
            
            //rewriteWithTag(tagi, outputName);
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void createBody2(File file, String outputName) {
        System.out.println(outputName);
        
        long lineNumber = 1;
        String[] indexerRow = new String[4];
        String filename = outputName;
        int part = 0;
        
        boolean newrow_flag = true;
        boolean content_flag = false;
        boolean tag_flag = false;
        try {
            InputStream inputStream = new FileInputStream(file);
            BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream, "iso-8859-2"));//new FileReader(path)
            BufferedWriter out = new BufferedWriter(new FileWriter("parsed_files/"+outputName + ".xml"));
            String line;
            String tag = "";
            String cont = "";
            int counter = 0;
            
            while ((line = reader.readLine()) != null) {
                line = line.trim();
                if (newrow_flag && line.matches("Path:.*")) {
                    indexerRow[0] = Long.toString(index);
                    indexerRow[1] = Long.toString(lineNumber);
                    out.write("<sphinx:document id=\"" + index + "\">\n");
                    lineNumber++;
                    newrow_flag = false;
                    tag_flag = true;
                }
                if (content_flag) {
                    if (line.contains("-=-=-=-=-=-=-=-=============----------===")) {
                        if (counter < contentLimit) {
                            out.write("<content><![CDATA[[");
                            out.write(cont);
                            out.write("\n]]></content>\n");
                            lineNumber = lineNumber+ counter + 2;
                        }
                        out.write("</sphinx:document>\n");
                        
                        indexerRow[2] = Long.toString(lineNumber);
                        indexerRow[3] = filename;
                        lineNumber++;
                        indexer.add(indexerRow);
                        indexerRow = new String[4];
                        cont = "";
                        counter = 0;
                        index++;
                        if(lineNumber>150000){
                            part++;
                            lineNumber = 1;
                            out.close();
                            filename = outputName+ "_part" + part;
                            out = new BufferedWriter(new FileWriter("parsed_files/"+ filename + ".xml"));
                        }
                        
                        content_flag = false;
                        tag = "";
                        newrow_flag = true;
                        
                    } else if (counter < contentLimit) {
                        if (!line.startsWith(">") && line.length() > 1) {
                            line = replaceInLine(line);
                            cont = cont + "\n" + line;
                            counter++;
                        }
                    }
                    
                } else if(tag_flag){
                    line = replaceInLine(line);
                    
                   
                    
                    if (line.matches("^[A-Za-z]+[A-Za-z-]*: .*")) {


                        //if (line.indexOf(": ") > 0) {
                        if (!tag.equals("")) {
                            out.write("]]></" + tag + ">\n");
                        }
                        int separator = line.indexOf(": ");
                        tag = line.substring(0, line.indexOf(": ")).trim().toLowerCase();
                        if (!tagi.contains(tag)) {
                            tagi.add(tag);
                        }
                        out.write("<" + tag + "><![CDATA[["
                                + line.substring(separator + 1).trim());
                        lineNumber++;
                    } else if(line.matches("\\s.*")){
                        out.write(" "+line.trim());
                       
                    }
                    if (line.contains("Xref: ")) {
                        content_flag = true;
                        tag_flag = false;
                        out.write("]]></" + tag + ">\n");
			//tag = "";
                        //out.write("      <content>");
                    }
                }
            }
            //out.write("</sphinx:docset>\n");
            out.close();
            reader.close();
            
            //rewriteWithTag(tagi, outputName);
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void createTemplate2() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("template.xml"));

            out.write("<sphinx:schema>\n");   
            for (int i = 0; i < tagi.size(); i++) {
                out.write("<sphinx:field name=\"" + tagi.get(i) + "\"/>\n");
            }
            out.write("</sphinx:schema>\n");
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void createTemplate() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("template.xml"));

            out.write("<sphinx:schema>\n");   
            /*for (int i = 0; i < tagi.size(); i++) {
                out.write("<sphinx:field name=\"" + tagi.get(i) + "\"/>\n");
            }
            out.write("</sphinx:schema>\n");*/
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void closeTemplate() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("template.xml", true));

            out.write("</sphinx:schema>\n");
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void updateTemplate() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("template.xml", true));
 
            for (int i = tag_last_index; i < tagi.size(); i++) {
                out.write("<sphinx:field name=\"" + tagi.get(i) + "\"/>\n");
                tag_last_index++;
            }
            //tagi.clear();
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void initIndex() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("index.txt"));
             
            out.write("");
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static void createIndex() {
        try {
            BufferedWriter out = new BufferedWriter(new FileWriter("index.txt", true));

             
            for (int i = 0; i < indexer.size(); i++) {
                out.write(indexer.get(i)[0]+";"+indexer.get(i)[1]+";"+indexer.get(i)[2]+";"+indexer.get(i)[3]+"\n");
            }
            indexer.clear();
            out.close();
            
        } catch (Exception e) {
            System.err.format("Exception");
            e.printStackTrace();
        }
    }
    
    public static String replaceInLine(String line){
        
        line = line.replaceAll("<!\\[CDATA\\[\\[", "");
	line = line.replaceAll("\\[", "");
        line = line.replaceAll("\\]", "");
        return line;
    }
}
