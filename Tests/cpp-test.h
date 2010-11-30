#define DEFINE_TEST_RECURISVE_ONCE_ONLY 1
// #undef DEFINE_TEST_RECURISVE_ONCE_ONLY

#define MULTI_LINE_DEFINITION \
    a \
    b \
    c \
    d

#include "test-recursive.h"

#if 0
#warning reached
#else
#warning not reached
#endif

#warning __LINE__

#ifdef one
# define a 1
#else
# define b 1
# ifdef two
#  define c 1
# else
#  define d 1
# endif
# define e 1
# ifdef d
#  undef e
#  define e 2
# endif
#endif

#define FIRST_NAME aaron
#define LAST_NAME bockover
#define FULL_NAME FIRST_NAME LAST_NAME

#warning End \
    of \
    test