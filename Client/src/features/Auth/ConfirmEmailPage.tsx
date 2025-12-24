import React, { useEffect, useState } from "react";
import { useSearchParams, useNavigate } from "react-router";
import { useConfirmEmail, useResendConfirmationEmail } from "../hooks/useAccount";
import InvalidLinkView from "./components/confirmEmail/InvalidLinkView";
import ConfirmEmailContainer from "./components/confirmEmail/ConfirmEmailContainer";

const ConfirmEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const userId = searchParams.get("userId");
  const code = searchParams.get("code");
  const [email, setEmail] = useState<string | null>(null);

  const confirmEmailMutation = useConfirmEmail();
  const resendEmailMutation = useResendConfirmationEmail();

  useEffect(() => {
    if (
      userId &&
      code &&
      !confirmEmailMutation.isPending &&
      !confirmEmailMutation.isSuccess &&
      !confirmEmailMutation.isError
    ) {
      confirmEmailMutation.mutate(
        { userId, code },
        {
          onSuccess: (data) => {
            // If token is present, user is automatically logged in
            if (data.token) {
              // Redirect to videos page (home) after 2 seconds
              setTimeout(() => {
                navigate("/videos", {
                  replace: true
                });
              }, 2000);
            } else {
              // No token, redirect to login
              setTimeout(() => {
                navigate("/login", {
                  state: {
                    message: "Email confirmed successfully! You can now log in."
                  }
                });
              }, 2000);
            }
          }
        }
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId, code]);

  const handleResendEmail = (emailToResend: string) => {
    resendEmailMutation.mutate(emailToResend);
  };

  if (!userId || !code) {
    return <InvalidLinkView />;
  }

  return (
    <ConfirmEmailContainer
      isPending={confirmEmailMutation.isPending}
      isSuccess={confirmEmailMutation.isSuccess}
      isError={confirmEmailMutation.isError}
      data={confirmEmailMutation.data}
      error={confirmEmailMutation.error as Error | null}
      email={email}
      onEmailChange={setEmail}
      resendEmailMutation={resendEmailMutation}
      onResend={handleResendEmail}
    />
  );
};

export default ConfirmEmailPage;

