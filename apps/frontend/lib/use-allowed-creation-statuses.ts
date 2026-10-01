import * as React from 'react';
import {
  CREATION_STATUSES,
  type CreationStatus,
  getMyAllowedStatuses,
} from './api/role-doc-policies';

interface State {
  statuses: CreationStatus[];
  loading: boolean;
}

/**
 * Fetches the creation statuses the current user is allowed to set for a given
 * document type. Falls back to ['DRAFT'] if the call fails or returns empty.
 */
export function useAllowedCreationStatuses(documentType: string): State {
  const [state, setState] = React.useState<State>({ statuses: ['DRAFT'], loading: true });

  React.useEffect(() => {
    let cancelled = false;
    setState((s) => ({ ...s, loading: true }));
    getMyAllowedStatuses(documentType)
      .then((list) => {
        if (cancelled) return;
        const valid = list.filter((s) => (CREATION_STATUSES as readonly string[]).includes(s));
        setState({ statuses: valid.length ? valid : ['DRAFT'], loading: false });
      })
      .catch(() => {
        if (!cancelled) setState({ statuses: ['DRAFT'], loading: false });
      });
    return () => { cancelled = true; };
  }, [documentType]);

  return state;
}
