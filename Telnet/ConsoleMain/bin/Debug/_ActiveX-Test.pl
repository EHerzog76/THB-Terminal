#!/usr/bin/perl -w
#package THBNetLib;

use strict;
# use warnings;
no strict 'refs';

use Getopt::Long;
use Class::Struct;
use Win32::OLE;
#use Win32::OLE::Const 'THB.Terminal.TelnetSSHLogin';
#$Win32::OLE::Warn = 3;                                # die on errors...

use constant DEBUG => 1;
# Usage: $0 [options] machine1, machine2, ...

=pod Prompt
Prompt   (?m:^[\w.-]+\s?(?:\(config[^\)]*\))?\s?[\$#>]\s?(?:\(enable\))?\s*$)
(?m:                  # Net::Telnet doesn't accept quoted regexen (i.e. qr//)
                        # so we need to use an embedded pattern-match modifier
                        # to treat the input as a multiline buffer.
    ^                   # beginning of line
      [\w.-]+           # router hostname
      \s?               # optional space
      (?:               # Strings like "(config)" and "(config-if)", "(config-line)",
                        # and "(config-router)" indicate that we're in privileged
        \(config[^\)]*\) # EXEC mode (i.e. we're enabled).
      )?                # The middle backslash is only there to appear my syntax
                        # highlighter.
      \s?               # more optional space
      [\$#>]            # Prompts typically end with "$", "#", or ">". Backslash
                        # for syntax-highlighter.
      \s?               # more space padding
      (?:               # Catalyst switches print "(enable)" when in privileged
        \(enable\)      # EXEC mode.
      )?
      \s*               # spaces before the end-of-line aren't important to us.
    $                   # end of line
  )                     # end of (?m:
=cut Prompt

    #my $shell = Win32::OLE->new('THB.Terminal');
    my $shell = Win32::OLE->new('THB.Terminal.TelnetSSHLogin');
    $shell->open("", "", "172.25.156.2", 0, "telnet", "", "", "");
    
    my $strOutput = $shell->WaitFor([ "Username:", "Login:", "Password:", "Press any key to continue" ], 0, 0);
    if ((!defined($strOutput)) || ($strOutput =~ /^$/)) {
        exit(1);
    }

    if ($strOutput =~ /press any key to continue/i) {
        $shell->print(" ");
        $strOutput = $shell->WaitFor([ "username:", "login:", "password:" ], 0, 0);
        if ( (!defined($strOutput)) || ($strOutput =~ /^$/)) { exit(1); }
    }

    if(($strOutput =~ /username:/i) || ($strOutput =~ /login:/i)) {
        $shell->print("UserName");
        $strOutput = $shell->WaitForString("Password:");
        if ((!defined($strOutput)) || ($strOutput =~ /^$/)) { exit(1); }

        $strOutput = $shell->cmd("Pwd1***");
    } elsif($strOutput =~ /password:/i) {
        $strOutput = $shell->cmd("p0st");
    } else {
        print("Error: Found no Loginprompt.");
        exit(1);
    }

    if ((!defined($strOutput)) || ($strOutput =~ /^$/)) {
        print("Error: Found no Prompt after Login.");
    }
    print($shell->ShowScreen());

    $shell = undef;
exit(0);
