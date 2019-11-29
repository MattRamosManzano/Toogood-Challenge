# Toogood-Challenge
The assignment given to me by Toogood Financial Systems

I assumed csv files were already read and are thus a string.

I assumed the string representing the file would look as follows:

--------------------------------

Col1,Col2,Col3,Col4

123,abc,456,def

789,zyx,654,wvu


instead of

Col1

Col2

Col3

Col4

123

abc

456

def

789

zyx

654

wvu


-----------------------------

Blanks in the target/output file will be blank: 123,,456,def

The target/output will have all records from the source.

I assumed inputs are valid.

The dictionary I used for formatting is defined as such: 

  {key = target column name, value = tuple(source column index, lambda function(str, str) returning string)

The order for the dictionary dictates the target's column order.
