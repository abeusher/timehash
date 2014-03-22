package com.hgg.tiger.utils;
/*
Copyright (c) 2014, HumanGeo
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of HumanGeo nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL HUMANGEO BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

import org.apache.commons.lang.StringUtils;

public class TimeHash {
	private static final String base32 = "01abcdef";
	private static Map<String, Integer> _decodemap = new HashMap<String, Integer>();
	static {
		String base32 = "01abcdef";
		HashMap<String, Integer> aMap = new HashMap<String, Integer>();
		for (int i = 0; i < base32.length(); i++) {
			aMap.put(Character.toString(base32.charAt(i)), new Integer(i));
			_decodemap = Collections.unmodifiableMap(aMap);
		}
		
	}
	private static final double timeIntervalStart = 0.0; //from January 1, 1970
	private static final double timeIntervalEnd = 4039372800.0; //to January 1, 2098

	public static HashMap<String,Double> decode_exactly(String timehash) {
		double start = timeIntervalStart;
		double end = timeIntervalEnd;
		double timeError = (start + end)/2; //this constant is used to calculate 
																	//the potential time error defined by 
																	//a particular number of characters
		for (int i = 0; i < timehash.length(); i++) {
			char c = timehash.charAt(i);
			Integer cd = _decodemap.get(Character.toString(c));
			
			int[] maskArray = {4, 2, 1};
			for (int j = 0; j < 3; j++) {
				timeError = timeError/2;
				double mid = (start + end)/2;
				if ((cd & maskArray[j]) == 0) {
					end = mid;
				} else {
					start = mid;
				}
			}
		}
		double timeValue = (start + end)/2;
		
		HashMap<String,Double> result = new HashMap<String,Double>();
		result.put("center", timeValue);
		result.put("error", timeError);
		result.put("start", timeValue-timeError);
		result.put("end", timeValue-timeError);
		return result;
	}
	
	public static double decode(String timehash) {
		return decode_exactly(timehash).get("center"); 
	}
	
	public static ArrayList<String> getTimeStampBetween(String start, String end) {
		ArrayList<String> timestamps = new ArrayList<String>();
		
		SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");
		try {
			Date startDate = (Date) formatter.parse(start);
			Date endDate = (Date) formatter.parse(end);
			long interval = 24 * 3600 * 1000;		// a day in milliseconds
			long curTime = startDate.getTime();
			long endTime = endDate.getTime();
			while (curTime <= endTime) {
				timestamps.add(formatter.format(new Date(curTime)));
				curTime += interval;
			}
		} catch (ParseException e) {
			System.out.println(e.getMessage());
		}
		return timestamps;
	}
	
	public static String encode(double epochTime, int precision) {
		double start = timeIntervalStart;
		double end = timeIntervalEnd;
		int[] bitArray = {4, 2, 1};
		
		String timehash = "";
		int bit = 0;
		int ch = 0;
		while (timehash.length() < precision) {
			double mid = (start + end)/2;
			if (epochTime > mid) {
				ch = ch | bitArray[bit];
				start = mid;
			} else {
				end = mid;
			}
			
			if (bit < 2) {
				bit = bit + 1;
			} else {
				timehash = timehash + base32.substring(ch, ch+1);
				bit = 0;
				ch = 0;
			}
		}
		return timehash;
	}
	
	public static void main(String[] args) {
		String timehash = "addcf1";
		System.out.println(TimeHash.decode(timehash));
		double epochtime = 1369708813.0439758;
		System.out.println(TimeHash.encode(epochtime,10));
	}
}
