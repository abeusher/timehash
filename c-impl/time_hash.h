#ifndef TIME_HASH_H
#define TIME_HASH_H

#ifdef __cplusplus
extern "C" {
#endif

#define CHAR_TO_IDX(A) ((int)A > 49) ? ((int)A - 95) : ((int)A - 48)

	struct time_hash {
		char*			hash_code;
		double			center;
		double			error;
	};

	void init_time_hash();
	
	int validate_time_hash(const char* hash_code);

	int encode_time_hash(double epoch_time, int precision, char* buffer);

	int decode_exactly_time_hash(const char* hash_code, struct time_hash* th);

	double decode_time_hash(const char* hash_code);

	int before_time_hash(const char* hash_code, char* buffer);

	int after_time_hash(const char* hash_code, char* buffer);

#ifdef __cplusplus
}
#endif

#endif