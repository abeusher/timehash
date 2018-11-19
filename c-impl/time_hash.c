#include "time_hash.h"
#include <stdio.h>

const char*			BASE32_TH = "01abcdef";
const char*			BEFORE_TH = "f01abcde";
const char*			AFTER_TH = "1abcdef0";
const int			MASK_ARR_TH[] = { 4, 2, 1 };

const double		TIME_INTERVAL_START_TH = 0.0;
const double		TIME_INTERVAL_END_TH = 4039372800.0;

char				NEIGHBOR_MAP_TH[8][2];

void init_time_hash() {
	for (int i = 0; i < 8; i++) {
		NEIGHBOR_MAP_TH[CHAR_TO_IDX(BASE32_TH[i])][0] = BEFORE_TH[i];
		NEIGHBOR_MAP_TH[CHAR_TO_IDX(BASE32_TH[i])][1] = AFTER_TH[i];
	}
}

int validate_time_hash(const char* hash_code) {
	int len = strlen(hash_code);
	int found = -1;
	for (int i = 0; i < len; i++) {
		for (int j = 0; j < len; j++) {
			if (hash_code[i] == BASE32_TH[j]) {
				found = 1;
				break;
			}
		}
		if (found == 1) return 0;
	}
	return -1;
}

int encode_time_hash(double epoch_time, int precision, char* buffer) {
	if (precision <= 0) return -1;
	if (buffer == NULL) return -2;
	if (strlen(buffer) != precision) return -3;

	double start = TIME_INTERVAL_START_TH;
	double end = TIME_INTERVAL_END_TH;

	int thlen = 0;
	int bit = 0;
	int ch = 0;

	while (thlen < precision) {
		double mid = (start + end) * 0.5;
		if (epoch_time > mid) {
			ch |= MASK_ARR_TH[bit];
			start = mid;
		}
		else {
			end = mid;
		}

		if (bit < 2) {
			bit++;
		}
		else {
			buffer[thlen] = BASE32_TH[ch];
			thlen++;
			bit = 0;
			ch = 0;
		}
	}

	return 0;
}

int decode_exactly_time_hash(const char* hash_code, struct time_hash* th) {
	if (hash_code == NULL) return -1;
	if (th == NULL) return -2;
	
	double start = TIME_INTERVAL_START_TH;
	double end = TIME_INTERVAL_END_TH;
	double time_error = (start + end) * 0.5;

	int len = strlen(hash_code);
	for (int i = 0; i < len; i++) {
		char c = hash_code[i];
		int cd = CHAR_TO_IDX(c);

		for (int j = 0; j < 3; j++) {
			time_error = time_error * 0.5;
			double mid = (start + end) * 0.5;

			if ((cd & MASK_ARR_TH[j]) == 0) {
				end = mid;
			}
			else {
				start = mid;
			}
		}
	}

	double time_val = (start + end) * 0.5;
	th->hash_code = hash_code;
	th->center = time_val;
	th->error = time_error;
	return 0;
}

double decode_time_hash(const char* hash_code) {
	struct time_hash th;
	if (decode_exactly_time_hash(hash_code, &th) == 0) {
		return th.center;
	}
	return -1.0;
}

int before_or_after_th(const char* hash_code, char* buffer, int neighbor) {
	if (hash_code == NULL) return -1;
	if (buffer == NULL) return -2;

	int i = 1;
	int len = strlen(hash_code);
	int blen = strlen(buffer);

	if (len != blen) return -3;

	char* reversed = (char*)malloc((len + 1) * sizeof(char));

	strcpy(reversed, hash_code);
	int j = 0, k = len - 1;
	while (j < k) {
		char tmp = reversed[j];
		reversed[j] = reversed[k];
		reversed[k] = tmp;
		j++;
		k--;
	}

	int succeeded = -4;

	char cmp = neighbor == 0 ? '0' : 'f';
	for (int m = 0; m < len; m++) {
		char c = reversed[m];
		if (c != cmp) {
			int pos = len - i;
			memcpy(buffer, hash_code, pos);
			buffer[pos] = NEIGHBOR_MAP_TH[CHAR_TO_IDX(c)][neighbor];
			for (int n = 0; n < i - 1; n++) {
				buffer[pos + n + 1] = neighbor == 0 ? 'f' : '0';
			}
			succeeded = 0;
			break;
		}
		else {
			i++;
		}
	}

	free(reversed);
	return succeeded;
}

int before_time_hash(const char* hash_code, char* buffer) {
	return before_or_after_th(hash_code, buffer, 0);
}

int after_time_hash(const char* hash_code, char* buffer) {
	return before_or_after_th(hash_code, buffer, 1);
}
