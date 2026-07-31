import { useEffect, useRef } from "react";

type UseInfiniteScrollOptions = {
  hasMore: boolean;
  isLoading: boolean;
  onLoadMore: () => void;
};

export function useInfiniteScroll({ hasMore, isLoading, onLoadMore }: UseInfiniteScrollOptions) {
  const triggerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const trigger = triggerRef.current;

    if (!trigger || !hasMore || isLoading) {
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          onLoadMore();
        }
      },
      { rootMargin: "160px 0px" },
    );

    observer.observe(trigger);

    return () => {
      observer.disconnect();
    };
  }, [hasMore, isLoading, onLoadMore]);

  return triggerRef;
}
