#define DEFINE_TEST_RECURISVE_ONCE_ONLY 1
#undef DEFINE_TEST_RECURISVE_ONCE_ONLY

// #define GL_TEST 1

#ifdef GL_TEST
#  include "gl.h"
#endif

#include "test-recursive.h"

#warning "End of Test"