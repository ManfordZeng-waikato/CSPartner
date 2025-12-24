import React, { useEffect, useState, useRef } from "react";
import { useSearchParams } from "react-router";
import { useResendConfirmationEmail } from "../hooks/useAccount";
import InvalidEmailView from "./components/checkEmail/InvalidEmailView";
import CheckEmailContainer from "./components/checkEmail/CheckEmailContainer";

const CheckEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email");
  const autoSend = searchParams.get("autoSend") === "true";
  const [countdown, setCountdown] = useState(60); // 60 seconds countdown
  const canResend = countdown === 0;
  const [resendEmail, setResendEmail] = useState<string>(email || "");
  const hasAutoSent = useRef(false);

  const resendEmailMutation = useResendConfirmationEmail();

  // Auto-send confirmation email when coming from login
  useEffect(() => {
    if (email && autoSend && !hasAutoSent.current) {
      hasAutoSent.current = true;
      resendEmailMutation.mutate(email, {
        onSuccess: () => {
          // Reset countdown after successful send
          setCountdown(60);
        }
      });
    }
  }, [email, autoSend, resendEmailMutation]);

  useEffect(() => {
    // Start countdown timer
    if (countdown > 0) {
      const timer = setTimeout(() => {
        setCountdown((prev) => prev - 1);
      }, 1000);
      return () => clearTimeout(timer);
    }
  }, [countdown]);

  const handleResendEmail = (emailToResend: string) => {
    resendEmailMutation.mutate(emailToResend, {
      onSuccess: () => {
        // Reset countdown after successful resend
        setCountdown(60);
      }
    });
  };

  if (!email) {
    return <InvalidEmailView />;
  }

  return (
    <CheckEmailContainer
      email={email}
      countdown={countdown}
      canResend={canResend}
      resendEmail={resendEmail}
      onResendEmailChange={setResendEmail}
      resendEmailMutation={resendEmailMutation}
      onResend={handleResendEmail}
    />
  );
};

export default CheckEmailPage;


