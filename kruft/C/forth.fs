( This File is to be executed by bfc - a legs forth cross compiler.
 bfc is not a true forth, as it lacks true extendable immediate mode.
 you will notice that regular old "exit" finishes most definitions,
 but not all - As a few definitions are allowed to "fall-through" to
 the next defined words.

 Words ending in ' are prime words - these are the original "doers" of
 defered words, that can be redefined later - which is most of the
 global variables state, cp, dh, and the replaceable parts of the
 "outer interpreter".  Legs uses an inherent Direct Threaded Code , or
 DTC model, and words are not natively redirectable without adding a
 wrapper word.
 
 You will notice a lack of text output words.  We don't need them for
 this file - we're just trying to produce a self-hosting forth and a
 few extension hooks for now, nothing more.  bfc provides a basic
 debugger allowing one to debug this code without defining output words.

)


p: ff debug \ toggle debugging

: dovar pull ;	     			\ the variable "doer"
: cell+	cell + ;			\ increments by a cell
: char+ char + ;			\ increments by a char
: !+    over ! cell+ ; ( a x -- a )	\ stores and inc. a
: c!+   over c! char+ ;    	     	\ cstores and inc. a
: @+    dup cell+ swap @ ; ( a -- a x ) \ fetches and inc. address
: c@+   dup char+ swap c@ ; 	      	\ cfetches and inc. address
: cp0	4 ;	      			\ starting cp value
: cp'   cp0 ;				\ compilation pointer
: cp 	cp' ;  			\ compilation pointer
: here	cp @ ;			\ current compilation pointer
: , 	here swap !+ cp ! ;		\ appends cell
: c,	here swap c!+ cp ! ;		\ appends char
: 0=	if 0 else -1 then ;		\ tests for 0
: 0<    mint and if -1 else 0 then ; \ tests for negative
: neg	com 1+ ;			\ negate
: -	neg + ;			\ subtract
: bl?	21 - 0< ;                    \ tests for whitespace
: rot	push swap pull swap ;	\ rotates top three cells
: 3drop	drop				\ drops three cells
: 2drop drop drop ;			\ drops two cells
: 2dup	over over ;			\ dups two cells
: tuck	swap over ;			\ tuck TOS under NOS
: nip	swap drop ;			\ removes NOS from stack
: -rot  rot rot ;                    \ bury TOS three deep
: u<	2dup xor 0< if nip 0< exit then - 0< ;
: state dovar # -1 ;		   	\ state of compiler
: ekey  key dup emit ;     		\ gets a key and echoes 
: +!	tuck @ + swap ! ; ( x a -- )	\ increment var by x

: tib ( -- a ) \ address of text input buffer
    here 100 + ;

: word' ( -- ca ) \ get next word from input stream
	tib 0 !+ 
	begin ekey dup bl? while drop repeat
	begin c!+ 1 tib +! ekey dup bl? until 2drop tib ; 

: word  word' ; 

: unloop
    pull pull drop push ;

: s=	( ca ca -- f ) \ compares string for equality
  	@+ rot @+ rot over - if 3drop 0 exit then
	for c@+ rot c@+ rot - if 2drop 0 unloop exit then
	next 2drop -1 ;

     
( 
Dictionary Format

offset	size	what
0	2	link
2	2	xt
4	2	flags
6	?	ca of name

)

: dh		6 ;		\ dictionary head
: latest	dh @ ;		\ latest word
: >name		cell+ 		\ returns ca of entry
: >flag		cell+ 		\ returns address of flags of entry
: >xt		cell+ ; 	\ returns address of xt of entry

: dfind  ( ca lh -- da )	\ find dictionary struct, or 0 if not found
	@ begin dup while 2dup >name s= if nip exit then @ repeat nip ;

: find  ( ca -- ca 0 | xt 1 | xt -1 ) \ find and xt
  	dup dh dfind dup 0= if exit then
	nip dup >xt @ swap >flag @ if 1 else -1 then ;

: [		-1 state ! ; immediate

: ]		0 state ! ; 


: ?dup ( x -- ? ) \ duplicates TOS if TOS is not zero
  dup if dup then ;

: s,  ( ca -- )           \ compile string
  @+ dup , for c@+ c, next drop ;

: header' ( "name" -- )	    \ makes a header
    here latest , dh !
    here 0 dup , , 
    word s,
    here swap !
;

: header  header' ;

: :            ( -- )       \ make a definition
  header ] ;

: ; immediate ( -- ) 
  lit exit , [ ;

: within ( a b c -- ) \ returns true if a is between b and c
     over - push - pull u< ;


: atou ( c -- x ) \ convert asci to a int - -1 on conversion err
     dup 2f 3a within if 30 - exit then
     dup 60 67 within if 57 - exit then
     drop -1 ;

: >num' ( ca -- n f )
  @+ over c@ 2d - 0= if 1- swap char+ swap -1 else 0 then -rot
  0 swap for shl shl shl shl swap c@+ 
  atou dup 0< if 2drop nip 0 unloop exit then
  rot + next nip swap if neg then -1 ;

: >num  >num' ;

: wnf' ( ca -- )
  3f emit d emit drop begin again ;

: wnf wnf' ;

: \ begin ekey dup d - 0= swap a - 0= or until ; immediate


: interpret ( -- ) \ interprets source until error or out of words
    begin
     	word dup @ 0= if drop exit then
        find ?dup if ( xt 1 | xt -1 )
	    0< 0= state @ or if exec else , then
	else ( ca )
            dup >num if 
	    	 nip state @ 0= if lit lit , , then
	    else
                 drop wnf exit
            then 
        then 
    again
     

: quit
  begin memz rp! interpret again
 
