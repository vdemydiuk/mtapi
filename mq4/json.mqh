// $Id: json.mqh 118 2014-02-28 23:50:37Z ydrol $
#ifndef YDROL_JSON_MQH
#define YDROL_JSON_MQH

#include <hash.mqh>

// (C)2014 Andrew Lord forex@NICKNAME@lordy.org.uk
// Parse a JSON String - Adapted for mql4++ from my gawk implementation
// ( https://code.google.com/p/oversight/source/browse/trunk/bin/catalog/json.awk )

/*
   TODO the constants true|false|null could be represented as fixed objects.
      To do this the deleting of _hash and _array must skip these objects.

   TODO test null

   TODO Parse Unicode Escape
*/

/*
   See json_demo for examples.

 This requires the hash.mqh ( http://codebase.mql4.com/9238 , http://lordy.co.nf/hash )



 */

/// Different types of JSON Values
enum ENUM_JSON_TYPE { JSON_NULL,JSON_OBJECT,JSON_ARRAY,JSON_NUMBER,JSON_STRING,JSON_BOOL };

class JSONString;
///
/// Generic class for all JSON types (Number, String, Bool, Array, Object )
/// It is a subclass of HashValue so it can be stored in an JSONObject hash
///
class JSONValue : public HashValue 
  {
private:
   ENUM_JSON_TYPE    _type;

public:
                     JSONValue() {}
                    ~JSONValue() {}
   ENUM_JSON_TYPE getType() { return _type; }
   void setType(ENUM_JSON_TYPE t) { _type=t; }

   /// True if JSONValue is a instance of JSONString
   bool isString() { return _type==JSON_STRING; }

   /// True if JSONValue is a instance of JSONNull 
   bool isNull() { return _type==JSON_NULL; }

   /// True if JSONValue is a instance of JSONObject
   bool isObject() { return _type==JSON_OBJECT; }

   /// True if JSONValue is a instance of JSONArray
   bool isArray() { return _type==JSON_ARRAY; }

   /// True if JSONValue is a instance of JSONNumber
   bool isNumber() { return _type==JSON_NUMBER; }

   /// True if JSONValue is a instance of JSONBool
   bool isBool() { return _type==JSON_BOOL; }

   /// Append the JSON text of this value to out. Override in child classes.
   /// Appending into one buffer keeps serialization linear in the output size.
   virtual void serialize(string &out) {}

   /// Return the JSON text of this value
   string toString()
     {
      string out="";
      serialize(out);
      return out;
     }

   /// Append s to out as a quoted JSON string, escaping " \ and control characters
   static void serializeString(string &out,string s)
     {
      StringAdd(out,"\"");
      int len=StringLen(s);
      int start=0;
      for(int i=0; i<len; i++)
        {
         ushort c=StringGetCharacter(s,i);
         if(c>=0x20 && c!='"' && c!='\\') continue;

         // flush the unescaped run before this character
         if(i>start) StringAdd(out,StringSubstr(s,start,i-start));
         switch(c)
           {
            case '"':  StringAdd(out,"\\\""); break;
            case '\\': StringAdd(out,"\\\\"); break;
            case '\n': StringAdd(out,"\\n"); break;
            case '\r': StringAdd(out,"\\r"); break;
            case '\t': StringAdd(out,"\\t"); break;
            case 8:    StringAdd(out,"\\b"); break;
            case 12:   StringAdd(out,"\\f"); break;
            default:   StringAdd(out,StringFormat("\\u%04X",(int)c));
           }
         start=i+1;
        }
      if(start==0) StringAdd(out,s); // nothing escaped - append as is
      else if(start<len) StringAdd(out,StringSubstr(s,start));
      StringAdd(out,"\"");
     }

   // Some convenience getters to cast to the subtype. - this is bad OO design!

   /// If this JSONValue is an instance of JSONString return the string (or cast will fail)
   string getString() { return((JSONString *)GetPointer(this)).getString(); }

   /// If this JSONValue is an instance of JSONNumber return the double (or cast will fail)
   double getDouble() { return((JSONNumber *)GetPointer(this)).getDouble(); }

   /// If this JSONValue is an instance of JSONNumber return the long (or cast will fail)
   long getLong() { return((JSONNumber *)GetPointer(this)).getLong(); }

   /// If this JSONValue is an instance of JSONNumber return the int (or cast will fail)
   int getInt() { return((JSONNumber *)GetPointer(this)).getInt(); }

   /// If this JSONValue is an instance of JSONBool return the bool (or cast will fail)
   bool getBool() { return((JSONBool *)GetPointer(this)).getBool(); }

   /// Get the string value of the JSONValue, without Program termination
   /// @param val : String object from which value will be extracted.
   /// @param out : The string than was extracted.
   /// @return true if OK else false
   static bool getString(JSONValue *val,string &out)
     {
      if(val!=NULL && val.isString()) 
        {
         out = val.getString();
         return true;
        }
      return false;
     }
   /// Get the bool value of the JSONValue, without Program termination
   /// @param val : String object from which value will be extracted.
   /// @param out : The bool than was extracted.
   /// @return true if OK else false
   static bool getBool(JSONValue *val,bool &out)
     {
      if(val!=NULL && val.isBool()) 
        {
         out = val.getBool();
         return true;
        }
      return false;
     }
   /// Get the double value of the JSONValue, without Program termination
   /// @param val : String object from which value will be extracted.
   /// @param out : The double than was extracted.
   /// @return true if OK else false
   static bool getDouble(JSONValue *val,double &out)
     {
      if(val!=NULL && val.isNumber()) 
        {
         out = val.getDouble();
         return true;
        }
      return false;
     }
   /// Get the long value of the JSONValue, without Program termination
   /// @param val : String object from which value will be extracted.
   /// @param out : The long than was extracted.
   /// @return true if OK else false
   static bool getLong(JSONValue *val,long &out)
     {
      if(val!=NULL && val.isNumber()) 
        {
         out = val.getLong();
         return true;
        }
      return false;
     }
   /// Get the int value of the JSONValue, without Program termination
   /// @param val : String object from which value will be extracted.
   /// @param out : The int than was extracted.
   /// @return true if OK else false
   static bool getInt(JSONValue *val,int &out)
     {
      if(val!=NULL && val.isNumber()) 
        {
         out = val.getInt();
         return true;
        }
      return false;
     }
  };
// -----------------------------------------

/// Class to represent a JSON String 
class JSONString : public JSONValue 
  {
private:
   string            _string;
public:
                     JSONString(string s) 
     {
      setString(s);
      setType(JSON_STRING);
     }
                     JSONString() 
     {
      setType(JSON_STRING);
     }
   string getString() { return _string; }
   void setString(string v) { _string=v; }
   void serialize(string &out)
     {
      JSONValue::serializeString(out,_string);
     }
  };
// -----------------------------------------

/// Class to represent a JSON Bool 
class JSONBool : public JSONValue 
  {
private:
   bool              _bool;
public:
                     JSONBool(bool b) 
     {
      setBool(b);
      setType(JSON_BOOL);
     }
                     JSONBool() 
     {
      setType(JSON_BOOL);
     }
   bool getBool() { return _bool; }
   void setBool(bool v) { _bool=v; }
   void serialize(string &out) { StringAdd(out,_bool ? "true" : "false"); }

  };
// -----------------------------------------

/// A JSON number may be internall replresented as either an MQL4 double or a long depending on how it was parsed. 
/// If one type is set the other is zeroed.
class JSONNumber : public JSONValue 
  {
private:
   long              _long;
   double            _dbl;
public:
                     JSONNumber(long l) 
     {
      _long= l;
      _dbl = 0;
      setType(JSON_NUMBER);
     }
                     JSONNumber(double d) 
     {
      _long= 0;
      _dbl = d;
      setType(JSON_NUMBER);
     }
   /// Get the long value, (cast) from internal double if necessary.
   long getLong() 
     {
      if(_dbl!=0) 
        {
         return (long)_dbl;
           } else {
         return _long;
        }
     }
   /// Get the int value, (cast) from internal value.
   int getInt() 
     {
      if(_dbl!=0) 
        {
         return (int)_dbl;
           } else {
         return (int)_long;
        }
     }
   /// Get the double value, (cast) from internal long if necessary.
   double getDouble()
     {
      if(_long!=0) 
        {
         return (double)_long;
           } else {
         return _dbl;
        }
     }
   void serialize(string &out)
     {
      // Favour the long
      if(_long!=0)
         StringAdd(out,(string)_long);
      else if(!MathIsValidNumber(_dbl))
         // Not valid in strict JSON, but accepted by Newtonsoft.Json and Python json.
         // null is not used because clients fail to deserialize it into double.
         StringAdd(out,(_dbl!=_dbl) ? "NaN" : (_dbl>0 ? "Infinity" : "-Infinity"));
      else if(_dbl!=0)
         StringAdd(out,(string)_dbl);
      else
         StringAdd(out,"0");
     }
  };
// -----------------------------------------

/// This class should not be necessary, but null is genrally infrequent so
/// I havent bothered to code it away yet.
class JSONNull : public JSONValue 
  {
public:
                     JSONNull()
     {
      setType(JSON_NULL);
     }
                    ~JSONNull() {}
   void serialize(string &out)
     {
      StringAdd(out,"null");
     }
  };

//forward declaration
class JSONArray;
/// This represents a JSONObject which is represented internally as a Hash
class JSONObject : public JSONValue 
  {
private:
   Hash             *_hash;
public:
                     JSONObject() 
     {
      setType(JSON_OBJECT);
     }
                    ~JSONObject() 
     {
      if(_hash != NULL) delete _hash;
     }
   /// Lookup key and get associated string value - halt program if wrong type(cast error) or doesnt exist(null pointer)
   string getString(string key)
     {
      return getValue(key).getString();
     }
   /// Lookup key and get associated bool value - halt program if wrong type(cast error) or doesnt exist(null pointer)
   bool getBool(string key)
     {
      return getValue(key).getBool();
     }
   /// Lookup key and get associated double value - halt program if wrong type(cast error) or doesnt exist(null pointer)
   double getDouble(string key)
     {
      return getValue(key).getDouble();
     }
   /// Lookup key and get associated long value - halt program if wrong type(cast error) or doesnt exist(null pointer)
   long getLong(string key)
     {
      return getValue(key).getLong();
     }
   /// Lookup key and get associated int value - halt program if wrong type(cast error) or doesnt exist(null pointer)
   int getInt(string key)
     {
      return getValue(key).getInt();
     }

   /// Lookup key and get associated string value, return false if failure.
   bool getString(string key,string &out)
     {
      return JSONValue::getString(getValue(key),out);
     }
   /// Lookup key and get associated bool value, return false if failure.
   bool getBool(string key,bool &out)
     {
      return JSONValue::getBool(getValue(key),out);
     }
   /// Lookup key and get associated double value, return false if failure.
   bool getDouble(string key,double &out)
     {
      return JSONValue::getDouble(getValue(key),out);
     }
   /// Lookup key and get associated long value, return false if failure.
   bool getLong(string key,long &out)
     {
      return JSONValue::getLong(getValue(key),out);
     }
   /// Lookup key and get associated int value, return false if failure.
   bool getInt(string key,int &out)
     {
      return JSONValue::getInt(getValue(key),out);
     }

   /// Lookup key and get associated array, NULL if not present. Cast failure if not an Array.
   JSONArray *getArray(string key)
     {
      return getValue(key);
     }
   /// Lookup key and get associated Object, NULL if not present. Cast failure if not an Object.
   JSONObject *getObject(string key)
     {
      return getValue(key);
     }
   /// Lookup key and get associated value - best for data whose structure might change as any type can safely be returned.
   JSONValue *getValue(string key)
     {
      if(_hash==NULL) 
        {
         return NULL;
        }
      return (JSONValue*)_hash.hGet(key);
     }

   /// Store the value against the specified key string - Used by the parser.
   void put(string key,JSONValue *v)
     {
      if(_hash==NULL) _hash=new Hash();
      // hPut returns the replaced value (duplicate key) - the hash no longer owns it, so delete it
      HashValue *old=_hash.hPut(key,v);
      if(old!=NULL && old!=v && CheckPointer(old)==POINTER_DYNAMIC) delete old;
     }
   void serialize(string &out)
     {
      StringAdd(out,"{");
      if(_hash!=NULL)
        {
         bool first=true;
         HashLoop l(_hash);
         for(; l.hasNext(); l.next())
           {
            if(!first) StringAdd(out,",");
            first=false;
            JSONValue::serializeString(out,l.key());
            StringAdd(out,":");
            JSONValue *v=(JSONValue *)(l.val());
            if(v==NULL)
               StringAdd(out,"null");
            else
               v.serialize(out);
           }
        }
      StringAdd(out,"}");
     }

   /// Return the internal Hash - Used by JSONIterator
   Hash *getHash() 
     {
      return _hash;
     }
  };
/// This is a JSONArray which is represented internally as a MQL4 dynamic array of JSONValue * 
class JSONArray : public JSONValue 
  {
private:
   int               _size;
   JSONValue        *_array[];
public:
                     JSONArray() 
     {
      setType(JSON_ARRAY);
     }
                    ~JSONArray() 
     {
      // clean up array
      for(int i=ArrayRange(_array,0)-1; i>=0; i--) 
        {
         if(CheckPointer(_array[i]) == POINTER_DYNAMIC ) delete _array[i];
        }
     }
   // Getters for Objects (key lookup ) --------------------------------------

   /// Lookup string value by array index - halt program if wrong type(cast error) or doesnt exist(null pointer)
   string getString(int index)
     {
      return getValue(index).getString();
     }
   /// Lookup bool value by array index - halt program if wrong type(cast error) or doesnt exist(null pointer)
   bool getBool(int index)
     {
      return getValue(index).getBool();
     }
   /// Lookup double value by array index - halt program if wrong type(cast error) or doesnt exist(null pointer)
   double getDouble(int index)
     {
      return getValue(index).getDouble();
     }
   /// Lookup long value by array index - halt program if wrong type(cast error) or doesnt exist(null pointer)
   long getLong(int index)
     {
      return getValue(index).getLong();
     }
   /// Lookup int value by array index - halt program if wrong type(cast error) or doesnt exist(null pointer)
   int getInt(int index)
     {
      return getValue(index).getInt();
     }

   /// Lookup JSONString by array index. NULL if not present. Cast failure if not an Object.
   bool getString(int index,string &out)
     {
      return JSONValue::getString(getValue(index),out);
     }
   /// Lookup JSONBool by array index. NULL if not present. Cast failure if not an Object.
   bool getBool(int index,bool &out)
     {
      return JSONValue::getBool(getValue(index),out);
     }
   /// Lookup JSONNumber by array index. NULL if not present. Cast failure if not an Object.
   bool getDouble(int index,double &out)
     {
      return JSONValue::getDouble(getValue(index),out);
     }
   /// Lookup JSONNumber by array index. NULL if not present. Cast failure if not an Object.
   bool getLong(int index,long &out)
     {
      return JSONValue::getLong(getValue(index),out);
     }
   /// Lookup JSONNumber by array index. NULL if not present. Cast failure if not an Object.
   bool getInt(int index,int &out)
     {
      return JSONValue::getInt(getValue(index),out);
     }

   /// Lookup array child by index, NULL if not present. Cast failure if not an Array.
   JSONArray *getArray(int index)
     {
      return getValue(index);
     }

   /// Lookup object child by index, NULL if not present. Cast failure if not an Array.
   JSONObject *getObject(int index)
     {
      return getValue(index);
     }
   /// The following method allows any type to be returned. Use this when parsing unpredictable data
   JSONValue *getValue(int index)
     {
      return _array[index];
     }

   /// Used by the Parser when building the array
   bool put(int index,JSONValue *v)
     {
      if(index>=_size) 
        {
         int oldSize = _size;
         // Reserve as much again as the new size, so the capacity doubles and filling
         // n elements one by one costs O(n) copying instead of O(n^2 / 30).
         int newSize = ArrayResize(_array,index+1,index+1>30 ? index+1 : 30);
         if(newSize <= index) return false;
         _size=newSize;

         // initialise
         for(int i=oldSize; i<newSize; i++) _array[i]=NULL;
        }
      // Delete old entry if any
      if(_array[index] != NULL) delete _array[index];

      //set new entry
      _array[index]=v;

      return true;
     }

   void serialize(string &out)
     {
      StringAdd(out,"[");
      for(int i=0; i<_size; i++)
        {
         if(i>0) StringAdd(out,",");
         if(_array[i]==NULL)
            StringAdd(out,"null");
         else
            _array[i].serialize(out);
        }
      StringAdd(out,"]");
     }

   int size() 
     {
      return _size;
     }
  };
/// Parse JSON text using a simple recursive descent parser
/// Exmaple
/// 
/// <pre>
///    string s = "{ \"firstName\": \"John\","+
///       " \"lastName\": \"Smith\","+
///       " \"age\": 25,"+
///       " \"address\": { \"streetAddress\": \"21 2nd Street\", \"city\": \"New York\", \"state\": \"NY\", \"postalCode\": \"10021\" },"+
///       " \"phoneNumber\": [ { \"type\": \"home\", \"number\": \"212 555-1234\" }, { \"type\": \"fax\", \"number\": \"646 555-4567\" } ],"+
///       " \"gender\":{ \"type\":\"male\" }  }";
///
///    JSONParser *parser = new JSONParser();
///
///    JSONValue *jv = parser.parse(s);
///
///    if (jv == NULL) {
///
///        Print("error:"+(string)parser.getErrorCode()+parser.getErrorMessage());
///
///    } else {
///
///        Print("PARSED:"+jv.toString());
///
///        if (jv.isObject()) {
///
///            JSONObject *jo = jv;
///
///            // Direct access - will throw null pointer if wrong getter used.
///
///            Print("firstName:" + jo.getString("firstName"));
///            Print("city:" + jo.getObject("address").getString("city"));
///            Print("phone:" + jo.getArray("phoneNumber").getObject(0).getString("number"));
///
///            // Safe access in case JSON data is missing or different.
///
///            if (jo.getString("firstName",s) ) Print("firstName = "+s);
///
///            // Loop over object returning JSONValue
///
///            JSONIterator *it = new JSONIterator(jo);
///            for( ; it.hasNext() ; it.next()) {
///                Print("loop:"+it.key()+" = "+it.val().toString());
///            }
///            delete it;
///        }
///        delete jv;
///    }
///    delete parser;
/// </pre>

class JSONParser 
  {
private:
   /// Current parse position
   int               _pos;
   /// The input string is expanded into an array of ushort (wchar)
   ushort            _in[];
   /// Length of string
   int               _len;
   /// The original input string
   string            _instr;

   int               _errCode;
   string            _errMsg;

   void setError(int code=1,string msg="unknown error") 
     {
      _errCode|=code;
      if(_errMsg=="") 
        {
         _errMsg="JSONParser::Error "+msg;
           } else {
         _errMsg=_errMsg+"\n"+msg;
        }
     }

   /// Parse a JSON Object
   JSONObject *parseObject()
     {
      JSONObject *o=new JSONObject();
      skipSpace();
      if(expect('{'))
        {
         skipSpace();
         if(!expectOptional('}')) // not an empty object
           {
            while(_errCode==0)
              {
               skipSpace();
               // A key is required here - this also rejects a trailing comma
               if(_in[_pos]!='"')
                 {
                  setError(2,"expected \" ");
                  break;
                 }

               // Read the key
               string key=parseString();

               if(_errCode!=0) break;

               skipSpace();

               if(!expect(':')) break;

               // read the value
               JSONValue *v = parseValue();
               if(_errCode != 0 ) break;

               o.put(key,v);

               skipSpace();

               if(!expectOptional(',')) break;
              }
            if(_errCode==0 && !expect('}'))
              {
               setError(2,"expected , or } ");
              }
           }
        }
      if(_errCode!=0) 
        {
         delete o;
         o=NULL;
        }
      return o;
     }

   bool isDigit(ushort c)
     {
      return c >= '0' && c <= '9';
     }

   /// Read 4 hex digits at _in[at] into out (for \uXXXX). False if invalid.
   bool parseHex4(int at,ushort &out)
     {
      if(at+4>_len) return false;
      uint v=0;
      for(int k=0; k<4; k++)
        {
         ushort h=_in[at+k];
         uint d;
         if(h>='0' && h<='9') d=(uint)(h-'0');
         else if(h>='a' && h<='f') d=(uint)(h-'a'+10);
         else if(h>='A' && h<='F') d=(uint)(h-'A'+10);
         else return false;
         v=v*16+d;
        }
      out=(ushort)v;
      return true;
     }

   /// Parse a number following the JSON grammar: -?int(.frac)?([eE][+-]?exp)?
   /// Integers are stored as long, anything with a fraction/exponent or too long for a long as double.
   JSONValue *parseNumber()
     {
      int start=_pos;
      int i=_pos;
      bool neg=false;
      if(_in[i]=='-')
        {
         neg=true;
         i++;
        }

      long l=0;
      int intStart=i;
      while(i<_len && isDigit(_in[i]))
        {
         l=l*10+(_in[i]-'0');
         i++;
        }
      int intDigits=i-intStart;
      bool ok=intDigits>0;
      bool isDouble=false;

      if(ok && i<_len && _in[i]=='.')
        {
         isDouble=true;
         i++;
         int fracStart=i;
         while(i<_len && isDigit(_in[i])) i++;
         ok=i>fracStart;
        }
      if(ok && i<_len && (_in[i]=='e' || _in[i]=='E'))
        {
         isDouble=true;
         i++;
         if(i<_len && (_in[i]=='+' || _in[i]=='-')) i++;
         int expStart=i;
         while(i<_len && isDigit(_in[i])) i++;
         ok=i>expStart;
        }

      if(!ok)
        {
         setError(3,"invalid number at position "+(string)start);
         return NULL;
        }
      _pos=i;

      // 18 digits always fit in a long, longer integers may overflow
      if(isDouble || intDigits>18)
         return new JSONNumber(StringToDouble(StringSubstr(_instr,start,i-start)));
      return new JSONNumber(neg ? -l : l);
     }

   void skipSpace() 
     {
      while(_in[_pos]==' ' || _in[_pos]=='\t' || _in[_pos]=='\r' || _in[_pos]=='\n') 
        {
         if(_pos>=_len) break;
         _pos++;
        }
     }

   bool expect(ushort c)
     {
      bool ret=false;
      if(c==_in[_pos]) 
        {
         _pos++;
         ret=true;
           } else 
           {
            setError(1,"expected "+ShortToString(c)+"("+(string)c+")"+
                     " got "+ShortToString(_in[_pos])+"("+(string)_in[_pos]+")");
        }
      return ret;
     }

   bool expectOptional(ushort c)
     {
      bool ret=false;
      if(c==_in[_pos]) 
        {
         _pos++;
         ret=true;
        }
      return ret;
     }

   string parseString()
     {
      string ret="";
      if(expect('"')) 
        {
         while(true) 
           {
            int end=_pos;
            while(end<_len && _in[end]!='"' && _in[end]!='\\') 
              {
               end++;
              }

            if(end>=_len) 
              {
               setError(2,"missing quote: end"+(string)end+":len"+(string)_len+":"+ShortToString(_in[_pos])+":"+StringSubstr(_instr,_pos,10)+"...");
               break;
              }
            // Check if character was escaped: \" \\ \/ \b \f \n \r \t \uXXXX
            if(_in[end]=='\\')
              {
               // Add partial string and get more
               if(end>_pos) StringAdd(ret,StringSubstr(_instr,_pos,end-_pos));
               end++;
               if(end>=_len)
                 {
                  setError(4,"parse error after escape");
                  break;
                 }
               ushort c=0;
               bool valid=true;
               switch(_in[end])
                 {
                  case '"':
                  case '\\':
                  case '/':
                     c=_in[end];
                     break;
                  case 'b': c=8; break; // backspace - 8
                  case 'f': c=12; break; // form feed 12
                  case 'n': c = '\n'; break;
                  case 'r': c = '\r'; break;
                  case 't': c = '\t'; break;
                  case 'u':
                     // UTF-16 code unit - surrogate pairs come as two \u escapes and
                     // combine naturally since MQL strings are UTF-16.
                     valid=parseHex4(end+1,c);
                     if(valid) end+=4;
                     break;
                  default:
                     valid=false;
                 }
               if(!valid)
                 {
                  setError(3,"invalid escape");
                  break;
                 }
               // \u0000 cannot be stored in an MQL string and is dropped
               if(c!=0) StringAdd(ret,ShortToString(c));
               _pos= end+1;
              }
            else // _in[end]=='"'
              {
               // End of string
               if(end>_pos) StringAdd(ret,StringSubstr(_instr,_pos,end-_pos));
               _pos=end+1;
               break;
              }
           }
        }
      if(_errCode!=0) 
        {
         ret=NULL;
        }
      return ret;
     }

   JSONValue *parseValue()
     {
      JSONValue *ret=NULL;
      skipSpace();
      string s;

      if(_in[_pos]=='[') 
        {

         ret=(JSONValue*)parseArray();

           } else if(_in[_pos]=='{') {

         ret=(JSONValue*)parseObject();

           } else if(_in[_pos]=='"') {

         s=parseString();
         ret=(JSONValue*)new JSONString(s);

           } else if(_in[_pos]=='-' || isDigit(_in[_pos])) {

         ret=parseNumber();

           } else if(_in[_pos]=='t' && StringSubstr(_instr,_pos,4)=="true") {

         ret=(JSONValue*)new JSONBool(true);
         _pos+=4;

           } else if(_in[_pos]=='f' && StringSubstr(_instr,_pos,5)=="false") {

         ret=(JSONValue*)new JSONBool(false);
         _pos+=5;

           } else if(_in[_pos]=='n' && StringSubstr(_instr,_pos,4)=="null") {

         ret=(JSONValue*)new JSONNull();
         _pos+=4;

           } else {

         setError(3,"error parsing value at position "+(string)_pos);

        }

      if(_errCode!=0 && ret!=NULL) 
        {
         delete ret;
         ret=NULL;
        }
      return ret;
     }

   JSONArray *parseArray()
     {
      JSONArray *ret=new JSONArray();

      int index=0;
      skipSpace();
      if(expect('['))
        {
         skipSpace();
         if(!expectOptional(']')) // not an empty array
           {
            while(_errCode==0)
              {
               skipSpace();

               // read the value - after a trailing comma this fails on ']'
               JSONValue *v = parseValue();
               if(_errCode != 0) break;

               if(!ret.put(index++,v))
                 {
                  delete v;
                  setError(3,"memory error adding "+(string)index);
                  break;
                 }

               skipSpace();

               if(!expectOptional(',')) break;
              }
            if(_errCode==0 && !expect(']'))
              {
               setError(2,"list: expected , or ] ");
              }
           }
        }

      if(_errCode!=0) 
        {
         delete ret;
         ret=NULL;
        }
      return ret;
     }
public:
   int getErrorCode()
     {
      return _errCode;
     }
   string getErrorMessage()
     {
      return _errMsg;
     }
   /// Parse a sequnce of characters and return a JSONValue.
   JSONValue *parse(
                    string s ///< Serialized JSON text
                    )
     {
      int inLen;
      JSONValue *ret=NULL;
      // No StringTrimLeft/Right: they trim in place in MQL5 but return a copy in MQL4.
      // Leading and trailing whitespace is skipped by the parser instead.
      _instr=s;
      _len=StringToShortArray(_instr,_in); // nul '0' is added to length
      _pos= 0;
      _errCode= 0;
      _errMsg = "";
      inLen=StringLen(_instr);
      if(_len!=inLen+1 /* nul */) 
        {
         setError(1,"unable to create array "+(string)inLen+" got "+(string)_len);
           } else {
         _len --;
         ret=parseValue();
         if(_errCode==0)
           {
            // Only whitespace may follow the value
            skipSpace();
            if(_pos<_len) setError(5,"unexpected text after JSON value");
           }
         if(_errCode!=0)
           {
            if(ret!=NULL)
              {
               delete ret;
               ret=NULL;
              }
            _errMsg=_errMsg+" at "+(string)_pos+" ["+StringSubstr(_instr,_pos,10)+"...]";
           }
        }
      return ret;
     }

  };
/// Class to iterate over a JSONObject (not a JSONArray)
class JSONIterator 
  {
private:
   HashLoop*_l;

public:
   // Create iterator and move to first item
                     JSONIterator(JSONObject *jo)
     {
      _l=new HashLoop(jo.getHash());
     }
                    ~JSONIterator()
     {
      delete _l;
     }
   // Check if more items
   bool hasNext()
     {
      return _l.hasNext();
     }

   // Move to next item
   void next() 
     {
      _l.next();
     }

   // Return item
   JSONValue *val()
     {
      return (JSONValue *) (_l.val());
     }

   // Return key
   string key()
     {
      return _l.key();
     }

  };
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
void json_demo()
  {
   string s="{ \"firstName\": \"John\","+
            " \"lastName\": \"Smith\","+
            " \"age\": 25,"+
            " \"address\": { \"streetAddress\": \"21 2nd Street\", \"city\": \"New York\", \"state\": \"NY\", \"postalCode\": \"10021\" },"+
            " \"phoneNumber\": [ { \"type\": \"home\", \"number\": \"212 555-1234\" }, { \"type\": \"fax\", \"number\": \"646 555-4567\" } ],"+
            " \"gender\":{ \"type\":\"male\" }  }";

   JSONParser *parser=new JSONParser();
   JSONValue *jv=parser.parse(s);
   Print("json:");
   if(jv==NULL) 
     {
      Print("error:"+(string)parser.getErrorCode()+parser.getErrorMessage());
        } else {
      Print("PARSED:"+jv.toString());
      if(jv.isObject()) 
        {
         JSONObject *jo=jv;

         // Direct access - will throw null pointer if wrong getter used.
         Print("firstName:"+jo.getString("firstName"));
         Print("city:"+jo.getObject("address").getString("city"));
         Print("phone:"+jo.getArray("phoneNumber").getObject(0).getString("number"));

         // Safe access in case JSON data is missing or different.
         if(jo.getString("firstName",s)) Print("firstName = "+s);

         // Loop over object returning JSONValue
         JSONIterator *it=new JSONIterator(jo);
         for(; it.hasNext(); it.next()) 
           {
            Print("loop:"+it.key()+" = "+it.val().toString());
           }
         delete it;
        }
      delete jv;
     }
   delete parser;
  }


#endif
//+------------------------------------------------------------------+
