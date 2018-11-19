#pragma once
#include <string>
#include <sstream>

namespace th {

	using namespace std;

	class time_hash {
		static const char*			BASE32;
		static const int			BASE32_LEN = 8;
		static const char*			BEFORE;
		static const char*			AFTER;
		static const int			MASK_ARR[3];

		static int					CHAR_TO_IDX(char c);
		static const char			NEIGHBOR_MAP[8][2];

		time_hash(const string hash_code, double center, double error)
			: hash_code(move(hash_code)), center(center), error(error) {}
	public:
		static const double			TIME_INTERVAL_START;
		static const double			TIME_INTERVAL_END;

		inline static bool			validate(const string& hash_code);
		inline static string		encode(double epoch_time, int precision);
		inline static time_hash		decode_exactly(const string& hash_code);
		inline static double		decode(const string& hash_code);
		inline static string		before(const string& hash_code);
		inline static string		after(const string& hash_code);

		const string				hash_code;
		const double				center;
		const double				error;

		time_hash(const string hash_code): time_hash(decode_exactly(hash_code)) {}
		time_hash(double epoch_time, int precision): time_hash(encode(epoch_time, precision)) {}
	};

	const char* time_hash::BASE32 = "01abcdef";
	const char* time_hash::BEFORE = "f01abcde";
	const char* time_hash::AFTER = "1abcdef0";
	const int time_hash::MASK_ARR[] = { 4, 2, 1 };

	const double time_hash::TIME_INTERVAL_START = 0.0;
	const double time_hash::TIME_INTERVAL_END = 4039372800.0;

	const char time_hash::NEIGHBOR_MAP[8][2] = {
		{ 'f', '1' },
		{ '0', 'a' },
		{ '1', 'b' },
		{ 'a', 'c' },
		{ 'b', 'd' },
		{ 'c', 'e' },
		{ 'd', 'f' },
		{ 'e', '0' }
	};

	inline int time_hash::CHAR_TO_IDX(char c) {
		return ((int)c > 49) ? ((int)c - 95) : ((int)c - 48);
	}

	inline bool time_hash::validate(const string& hash_code) {
		string b32(BASE32);
		for (int i = 0; i < hash_code.size(); i++) {
			if (b32.find(hash_code[i]) == string::npos) {
				return false;
			}
		}
		return true;
	}

	inline string time_hash::encode(double epoch_time, int precision) {
		double start = TIME_INTERVAL_START;
		double end = TIME_INTERVAL_END;

		string timehash;
		int bit = 0;
		int ch = 0;

		while (timehash.size() < precision) {
			double mid = (start + end) * 0.5;
			if (epoch_time > mid) {
				ch |= MASK_ARR[bit];
				start = mid;
			}
			else {
				end = mid;
			}

			if (bit < 2) {
				bit++;
			}
			else {
				timehash.append(&BASE32[ch], 1);
				bit = 0;
				ch = 0;
			}
		}

		return timehash;
	}

	inline time_hash time_hash::decode_exactly(const string& hash_code) {
		double start = TIME_INTERVAL_START;
		double end = TIME_INTERVAL_END;
		double time_error = (start + end) * 0.5;

		for (int i = 0; i < hash_code.size(); i++) {
			char c = hash_code[i];
			int cd = CHAR_TO_IDX(c);

			for (int j = 0; j < 3; j++) {
				time_error = time_error * 0.5;
				double mid = (start + end) * 0.5;

				if ((cd & MASK_ARR[j]) == 0) {
					end = mid;
				}
				else {
					start = mid;
				}
			}
		}

		double time_val = (start + end) * 0.5;

		return time_hash(hash_code, time_val, time_error);
	}

	inline double time_hash::decode(const string& hash_code) {
		return decode_exactly(hash_code).center;
	}

	inline string time_hash::before(const string& hash_code) {
		int i = 1;
		string reversed(hash_code);
		std::reverse(reversed.begin(), reversed.end());
		stringstream ret;

		bool succeeded = false;

		for (int j = 0; j < reversed.size(); j++) {
			char c = reversed[j];
			if (c != '0') {
				string padding(i - 1, 'f');
				int pos = reversed.size() - i;
				ret << hash_code.substr(0, pos) << NEIGHBOR_MAP[CHAR_TO_IDX(c)][0] << padding;
				succeeded = true;
				break;
			}
			else {
				i++;
			}
		}

		return succeeded ? ret.str() : hash_code;
	}

	inline string time_hash::after(const string& hash_code) {
		int i = 1;
		string reversed(hash_code);
		std::reverse(reversed.begin(), reversed.end());
		stringstream ret;

		bool succeeded = false;

		for (int j = 0; j < reversed.size(); j++) {
			char c = reversed[j];
			if (c != 'f') {
				string padding(i - 1, '0');
				int pos = reversed.size() - i;
				ret << hash_code.substr(0, pos) << NEIGHBOR_MAP[CHAR_TO_IDX(c)][1] << padding;
				succeeded = true;
				break;
			}
			else {
				i++;
			}
		}

		return succeeded ? ret.str() : hash_code;
	}
}